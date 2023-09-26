import { HttpClient, HttpErrorResponse, HttpHeaders, HttpResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Resource, ResourceDto } from '../models/resource';
import { lastValueFrom } from 'rxjs';
import { ListResource, ListResourceDto, ProblemDetails, ProblemDetailsDto } from '../../public-api';
import { ResourceFactory } from './resource-factory';

interface HttpClientOptions {
  headers?: HttpHeaders;
  responseType: 'json';
  observe: 'response';
}

@Injectable({
  providedIn: 'root'
})
/**
 * The `HalClient` class provides a client for interacting with a server that implements the Hypertext Application Language (HAL) specification.
 * It provides methods for retrieving resources, list resources, and forms resources, through GET, POST, PUT and DELETE requests.
 * No Methods should throw exceptions, but instead return an `HttpResponse` object that contains either the requested resource or a `ProblemDetails` object.
 * The `HalClient` class is intended to be used as a singleton service in your application.
 */
export class HalClient {

  constructor(private _httpClient: HttpClient) { }

  /**
   * The HttpClient instance used by the HAL client.
   */
  public get httpClient(): HttpClient { return this._httpClient; }

  
  /**
   * Retrieves a resource from the specified URI.
   * @param uri The URI of the resource to retrieve.
   * @param headers Optional HTTP headers to include in the request.
   * @returns A promise that resolves with an HTTP response containing the requested resource or a problem details object.
   */
  public async getResource<TDto>(uri: string, headers?: HttpHeaders): Promise<HttpResponse<Resource & TDto | ProblemDetails>> {
    return this.get<TDto, Resource & TDto>(uri, ResourceFactory.createResource, headers);
  }

  /**
   * Retrieves a list resource from the specified URI.
   * @param uri The URI of the list resource to retrieve.
   * @param headers Optional HTTP headers to include in the request.
   * @returns A Promise that resolves with an HttpResponse containing the retrieved list resource or a ProblemDetails object.
   * @template TListEntryDto The type of the DTO for the list entries.
   */
  public async getListResource<TListEntryDto>(uri: string, headers?: HttpHeaders): Promise<HttpResponse<ListResource<TListEntryDto & ResourceDto> | ProblemDetails>> {
    return this.get<ListResourceDto<TListEntryDto & ResourceDto>, ListResource<TListEntryDto & ResourceDto>>(uri, ResourceFactory.createListResource, headers);
  }

  /**
   * Retrieves a forms resource from the specified URI.
   * @param uri The URI of the forms resource to retrieve.
   * @param headers Optional HTTP headers to include in the request.
   * @returns A promise that resolves with an HTTP response containing the forms resource or a problem details object.
   * @template TDto The type of the DTO to deserialize from the response body. Most of the time this will be void, because the forms resource does not contain any data by itself, but in the template.
   */
  public async getFormsResource<TDto = void>(uri: string, headers?: HttpHeaders): Promise<HttpResponse<Resource & TDto | ProblemDetails>> {
    return this.get<TDto, Resource & TDto>(uri, ResourceFactory.createFormResource, headers);
  }

  /**
   * Sends a GET request to the specified URI and returns a promise that resolves with the response body as a resource object.
   * @param uri The URI to send the GET request to.
   * @param factory A factory function that creates a new instance of the resource object from the response body. Normally this is one of the factory functions of the ResourceFactory class.
   * @param headers Optional HTTP headers to include in the request.
   * @returns A promise that resolves with the response body as a resource object or a problem details object.
   */
  public async get<TDto, TResource extends Resource = Resource>(uri: string, factory: (dto: TDto & ResourceDto) => TResource & TDto, headers?: HttpHeaders): Promise<HttpResponse<TResource | ProblemDetails>> {
    const options = HalClient.createOptions(headers);
    let dtoResponse: HttpResponse<TDto & ResourceDto | ProblemDetailsDto> | undefined;
    try {
      dtoResponse = await lastValueFrom(this._httpClient.get<TDto & ResourceDto | ProblemDetailsDto>(uri, options));
    }
    catch (e) {
      if (e instanceof HttpErrorResponse || e instanceof HttpResponse)
        return HalClient.convertErrorResponse(e);
      else
        return HalClient.convertError(e, "GET", uri, options);
    }

    if (!dtoResponse)
      return HalClient.convertNoResponse("GET", uri, options);

    try {
      if (dtoResponse.ok)
        return HalClient.convertResponse(dtoResponse as HttpResponse<TDto & ResourceDto>, factory);

      return HalClient.convertErrorResponse(dtoResponse as HttpResponse<ProblemDetailsDto>);
    }
    catch (e) {
      return HalClient.convertFailedToConvertToProblemDetails(dtoResponse, e);
    }
  }
  
  /**
   * Sends a POST request to the specified URI and returns the response as a Resource object with the specified DTO type.
   * @param uri The URI to send the POST request to.
   * @param body The body of the POST request.
   * @param headers Optional headers to include in the request.
   * @returns A Promise that resolves with the HTTP response as a Resource object with the specified DTO type.
   */
  public async postAndGetResultAsResource<TDto>(uri: string, body: any, headers?: HttpHeaders): Promise<HttpResponse<Resource & TDto | ProblemDetails>> {
    return this.post<TDto, Resource & TDto>(uri, body, ResourceFactory.createResource, headers);
  }

  /**
   * Sends a POST request to the specified URI and returns the response as a ListResource object.
   * @param uri The URI to send the request to.
   * @param body The body of the request.
   * @param headers The headers to include in the request.
   * @returns A Promise that resolves with the HTTP response as a ListResource object or a ProblemDetails object.
   * @template TListEntryDto The type of the DTO for the list entry.
   */
  public async postAndGetResultAsListResource<TListEntryDto>(uri: string, body: any, headers?: HttpHeaders): Promise<HttpResponse<ListResource<TListEntryDto & ResourceDto> | ProblemDetails>> {
    return this.post<ListResourceDto<TListEntryDto & ResourceDto>, ListResource<TListEntryDto & ResourceDto>>(uri, body, ResourceFactory.createListResource, headers);
  }

  /**
   * Sends a POST request to the specified URI with the given body and headers, and returns the response as a Forms Resource.
   * @param uri The URI to send the request to.
   * @param body The body of the request.
   * @param headers The headers to include in the request.
   * @returns A Promise that resolves with the response as a Forms Resource.
   * @template TDto The type of the DTO to deserialize from the response body. Most of the time this will be void, because the forms resource does not contain any data by itself, but in the template.
   */
  public async postAndGetResultAsFormsResource<TDto = void>(uri: string, body: any, headers?: HttpHeaders): Promise<HttpResponse<Resource & TDto | ProblemDetails>> {
    return this.post<TDto, Resource & TDto>(uri, body, ResourceFactory.createFormResource, headers);
  }

  /**
   * Sends a POST request to the specified URI with the given body and headers.
   * @param uri The URI to send the request to.
   * @param body The body of the request.
   * @param factory A factory function that creates a new instance of the resource object from the response body. Normally this is one of the factory functions of the ResourceFactory class.
   * @param headers Optional headers to include in the request.
   * @returns A promise that resolves to an HttpResponse containing the resource or a ProblemDetails object.
   */
  public async post<TDto, TResource extends Resource = Resource>(uri: string, body: any, factory: (dto: TDto & ResourceDto) => TResource & TDto, headers?: HttpHeaders): Promise<HttpResponse<TResource | ProblemDetails>> {
    const options = HalClient.createOptions(headers);
    let dtoResponse: HttpResponse<TDto & ResourceDto | ProblemDetailsDto> | undefined;

    try {
      dtoResponse = await lastValueFrom(this._httpClient.post<TDto & ResourceDto | ProblemDetailsDto>(uri, body, options));
    }
    catch (e) {
      if (e instanceof HttpErrorResponse || e instanceof HttpResponse)
        return HalClient.convertErrorResponse(e);
      else
        return HalClient.convertError(e, "GET", uri, options);
    }

    if (!dtoResponse)
      return HalClient.convertNoResponse("GET", uri, options);

    try {
      if (dtoResponse.ok)
        return HalClient.convertResponse(dtoResponse as HttpResponse<TDto & ResourceDto>, factory);

      return HalClient.convertErrorResponse(dtoResponse as HttpResponse<ProblemDetailsDto>);
    }
    catch (e) {
      return HalClient.convertFailedToConvertToProblemDetails(dtoResponse, e);
    }
  }
  
  /**
   * Sends a PUT request to the specified URI and returns the response as a Resource object with the specified DTO type.
   * @param uri The URI to send the PUT request to.
   * @param body The body of the PUT request.
   * @param headers Optional headers to include in the request.
   * @returns A Promise that resolves with the HTTP response as a Resource object with the specified DTO type.
   */
  public async putAndGetResultAsResource<TDto>(uri: string, body: any, headers?: HttpHeaders): Promise<HttpResponse<Resource & TDto | ProblemDetails>> {
    return this.put<TDto, Resource & TDto>(uri, body, ResourceFactory.createResource, headers);
  }

  /**
   * Sends a PUT request to the specified URI and returns the response as a ListResource object.
   * @param uri The URI to send the request to.
   * @param body The body of the request.
   * @param headers The headers to include in the request.
   * @returns A Promise that resolves with the HTTP response as a ListResource object or a ProblemDetails object.
   * @template TListEntryDto The type of the DTO for the list entry.
   */
  public async putAndGetResultAsListResource<TListEntryDto>(uri: string, body: any, headers?: HttpHeaders): Promise<HttpResponse<ListResource<TListEntryDto & ResourceDto> | ProblemDetails>> {
    return this.put<ListResourceDto<TListEntryDto & ResourceDto>, ListResource<TListEntryDto & ResourceDto>>(uri, body, ResourceFactory.createListResource, headers);
  }

  /**
   * Sends a PUT request to the specified URI with the given body and headers, and returns the response as a Forms Resource.
   * @param uri The URI to send the request to.
   * @param body The body of the request.
   * @param headers The headers to include in the request.
   * @returns A Promise that resolves with the response as a Forms Resource.
   * @template TDto The type of the DTO to deserialize from the response body. Most of the time this will be void, because the forms resource does not contain any data by itself, but in the template.
   */
  public async putAndGetResultAsFormsResource<TDto = void>(uri: string, body: any, headers?: HttpHeaders): Promise<HttpResponse<Resource & TDto | ProblemDetails>> {
    return this.put<TDto, Resource & TDto>(uri, body, ResourceFactory.createFormResource, headers);
  }

  /**
   * Sends a PUT request to the specified URI with the provided body and headers.
   * @param uri The URI to send the request to.
   * @param body The body of the request.
   * @param factory A factory function that creates a new instance of the resource object from the response body. Normally this is one of the factory functions of the ResourceFactory class.
   * @param headers Optional headers to include in the request.
   * @returns A promise that resolves with the response object or a problem details object.
   */
  public async put<TDto, TResource extends Resource = Resource>(uri: string, body: any, factory: (dto: TDto & ResourceDto) => TResource & TDto, headers?: HttpHeaders): Promise<HttpResponse<TResource | ProblemDetails>> {
    const options = HalClient.createOptions(headers);
    let dtoResponse: HttpResponse<TDto & ResourceDto | ProblemDetailsDto> | undefined;

    try {
      dtoResponse = await lastValueFrom(this._httpClient.put<TDto & ResourceDto | ProblemDetailsDto>(uri, body, options));
    }
    catch (e) {
      if (e instanceof HttpErrorResponse || e instanceof HttpResponse)
        return HalClient.convertErrorResponse(e);
      else
        return HalClient.convertError(e, "GET", uri, options);
    }

    if (!dtoResponse)
      return HalClient.convertNoResponse("GET", uri, options);

    try {
      if (dtoResponse.ok)
      return HalClient.convertResponse(dtoResponse as HttpResponse<TDto & ResourceDto>, factory);

      return HalClient.convertErrorResponse(dtoResponse as HttpResponse<ProblemDetailsDto>);
    }
    catch (e) {
      return HalClient.convertFailedToConvertToProblemDetails(dtoResponse, e);
    }
  }

  /**
   * Sends a DELETE request to the specified URI and returns a Promise that resolves with an HttpResponse containing
   * either void or a ProblemDetails object.
   * @param uri - The URI to send the DELETE request to.
   * @param headers - Optional HttpHeaders to include in the request.
   * @returns A Promise that resolves with an HttpResponse containing either void or a ProblemDetails object.
   */
  public async delete(uri: string, headers?: HttpHeaders): Promise<HttpResponse<void | ProblemDetails>> {
    const options = HalClient.createOptions(headers);
    let response: HttpResponse<void | ProblemDetailsDto> | undefined;

    try {
      response = await lastValueFrom(this._httpClient.delete<void | ProblemDetailsDto>(uri, options));
    }
    catch (e) {
      if (e instanceof HttpErrorResponse || e instanceof HttpResponse)
        response = HalClient.convertErrorResponse(e);
      else
        return HalClient.convertError(e, "GET", uri, options);
    }

    if (!response)
      return HalClient.convertNoResponse("GET", uri, options);

    if (!response.ok) {
      try {
        const errorResponse = HalClient.convertErrorResponse(response as HttpResponse<ProblemDetailsDto>);

        return errorResponse;
      }
      catch (e) {
        return HalClient.convertErrorResponse(response as HttpResponse<ProblemDetailsDto>);
      }
    }

    return response as HttpResponse<void>;
  }

  private static createOptions(headers?: HttpHeaders): HttpClientOptions {
    headers?.append('Accept', 'application/hal+json')
    return {
      headers: headers,
      responseType: 'json',
      observe: 'response'
    }
  }

  private static convertResponse<TResource extends Resource, TDto extends ResourceDto>(response: HttpResponse<TDto>, factory: (dto: TDto) => TResource): HttpResponse<TResource | ProblemDetails> {
    if (response.body === null || response.body === undefined)
      return HalClient.convertEmptyResponse(response);

    const resource = factory(response.body);
    const resourceResponse = response.clone({ body: resource });

    return resourceResponse;
  }

  private static convertErrorResponse(response: HttpResponse<ProblemDetailsDto> | HttpErrorResponse): HttpResponse<ProblemDetails> {
    // HttpErrorResponse
    if (response instanceof HttpErrorResponse) {
      const dto: ResourceDto & ProblemDetailsDto = {
        _links: { self: [{ href: response.url ?? "" }] },
        title: response.statusText,
        status: response.status,
        detail: response.message,
        instance: response.url ?? ""
      };

      const resource = ResourceFactory.createProblemDetails(dto);
      const resourceResponse = new HttpResponse<ProblemDetails>({
        body: resource,
        headers: response.headers,
        status: response.status,
        statusText: response.statusText,
        url: response.url ?? ""
      });

      return resourceResponse;
    }

    // Empty response
    if (response.body === null || response.body === undefined)
      return HalClient.convertEmptyResponse(response);

    // HttpResponse<ProblemDetailsDto>
    const resource = ResourceFactory.createProblemDetails(response.body);
    const resourceResponse = response.clone({ body: resource });

    return resourceResponse;
  }

  private static convertEmptyResponse(response: HttpResponse<unknown>): HttpResponse<ProblemDetails> {
    const dto: ResourceDto & ProblemDetailsDto = {
      _links: { self: [{ href: response.url ?? "" }] },
      title: response.statusText,
      status: response.status,
      detail: "The server has returned an empty response.",
      instance: response.url ?? ""
    };

    const resource = ResourceFactory.createProblemDetails(dto);
    const resourceResponse = response.clone({ body: resource });

    return resourceResponse;
  }

  private static convertError(error: unknown, method: string, uri: string, options: HttpClientOptions, body?: any): HttpResponse<ProblemDetails> {
    const dto: ResourceDto & ProblemDetailsDto = {
      _links: { self: [{ href: uri }] },
      title: "An error occured.",
      status: 500,
      detail: `An error occured while executing the ${method}-request to ${uri} with options ${JSON.stringify(options)} and body ${JSON.stringify(body ?? "<empty>")}.`,
      instance: uri
    };

    const resource = ResourceFactory.createProblemDetails(dto);
    const resourceResponse = new HttpResponse<ProblemDetails>({
      body: resource,
      headers: options.headers,
      status: dto.status,
      statusText: dto.title,
      url: uri
    });

    return resourceResponse;
  }

  private static convertNoResponse(method: string, uri: string, options: HttpClientOptions, body?: any): HttpResponse<ProblemDetails> {
    const dto: ResourceDto & ProblemDetailsDto = {
      _links: { self: [{ href: uri }] },
      title: "An error occured.",
      status: 500,
      detail: `No response was returned while executing the ${method}-request to ${uri} with options ${JSON.stringify(options)} and body ${JSON.stringify(body ?? "<empty>")}.`,
      instance: uri
    };

    const resource = ResourceFactory.createProblemDetails(dto);
    const resourceResponse = new HttpResponse<ProblemDetails>({
      body: resource,
      headers: options.headers,
      status: dto.status,
      statusText: dto.title,
      url: uri
    });

    return resourceResponse;
  }

  private static convertFailedToConvertToProblemDetails(response: HttpResponse<unknown>, error: unknown): HttpResponse<ProblemDetails> {
    const dto: ResourceDto & ProblemDetailsDto = {
      _links: { self: [{ href: response.url ?? "" }] },
      title: "Error response is not problem details.",
      status: response.status,
      detail: `The server returned an error response, but it could not be converted to a ProblemDetails resource. The error was: ${JSON.stringify(error)}. The response body was: ${JSON.stringify(response.body)}.`,
      instance: response.url ?? ""
    };

    const resource = ResourceFactory.createProblemDetails(dto);
    const resourceResponse = new HttpResponse<ProblemDetails>({
      body: resource,
      headers: response.headers,
      status: dto.status,
      statusText: dto.title,
      url: response.url ?? ""
    });

    return resourceResponse;
  }
}
