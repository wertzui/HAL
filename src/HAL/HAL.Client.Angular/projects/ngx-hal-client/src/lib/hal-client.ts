import { HttpClient, HttpErrorResponse, HttpHeaders, HttpResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Resource, ResourceDto } from './Models/resource';

@Injectable({
  providedIn: 'root'
})
export class HalClient {

  constructor(private _httpClient: HttpClient) { }

  public get httpClient(): HttpClient { return this._httpClient; }

  public async get<TResource extends Resource, TError extends Resource>(uri: string, TResource: { new(): TResource }, TError: { new(): TError }, headers?: HttpHeaders): Promise<HttpResponse<TResource | TError>> {
    const options = HalClient.createOptions(headers);
    let dtoResponse: HttpResponse<ResourceDto> | undefined;
    try {
      dtoResponse = await this._httpClient.get<ResourceDto>(uri, options).toPromise();
    }
    catch (e) {
      if (e instanceof HttpErrorResponse)
        dtoResponse = HalClient.convertErrorResponse(e);
      else
        throw new Error(`GET ${uri} - options: ${JSON.stringify(options)} failed with error ${e}`);
    }
    if (!dtoResponse)
      throw new Error(`GET ${uri} - options: ${JSON.stringify(options)} did not return a response.`);
    const resourceResponse = HalClient.convertResponse <TResource | TError>(dtoResponse.ok ? TResource : TError, dtoResponse);
    return resourceResponse;
  }

  public async post<TResource extends Resource, TError extends Resource>(uri: string, body: unknown, TResource: { new(): TResource }, TError: { new(): TError }, headers?: HttpHeaders): Promise<HttpResponse<TResource | TError>> {
    const options = HalClient.createOptions(headers);
    let dtoResponse: HttpResponse<ResourceDto> | undefined;
    try {
      dtoResponse = await this._httpClient.post<TResource>(uri, body, options).toPromise();
    }
    catch (e) {
      if (e instanceof HttpErrorResponse)
        dtoResponse = HalClient.convertErrorResponse(e);
      else
        throw new Error(`POST ${uri} - options: ${JSON.stringify(options)} - body: ${body} failed with error ${e}`);
    }
    if (!dtoResponse)
      throw new Error(`POST ${uri} - options: ${JSON.stringify(options)} - body: ${body} did not return a response.`);
    const resourceResponse = HalClient.convertResponse<TResource | TError>(dtoResponse.ok ? TResource : TError, dtoResponse);
    return resourceResponse;
  }

  public async put<TResource extends Resource, TError extends Resource>(uri: string, body: unknown, TResource: { new(): TResource }, TError: { new(): TError }, headers?: HttpHeaders): Promise<HttpResponse<TResource | TError>> {
    const options = HalClient.createOptions(headers);
    let dtoResponse: HttpResponse<ResourceDto> | undefined;
    try {
      dtoResponse = await this._httpClient.put<TResource>(uri, body, options).toPromise();
    }
    catch (e) {
      if (e instanceof HttpErrorResponse)
        dtoResponse = HalClient.convertErrorResponse(e);
      else
        throw new Error(`PUT ${uri} - options: ${JSON.stringify(options)} - body: ${body} failed with error ${e}`);
    }
    if (!dtoResponse)
      throw new Error(`PUT ${uri} - options: ${JSON.stringify(options)} - body: ${body} did not return a response.`);
    const resourceResponse = HalClient.convertResponse<TResource | TError>(dtoResponse.ok ? TResource : TError, dtoResponse);
    return resourceResponse;
  }

  public async delete<TError extends Resource>(uri: string, TError: { new(): TError }, headers?: HttpHeaders): Promise<HttpResponse<void | TError>> {
    const options = HalClient.createOptions(headers);
    let response: HttpResponse<ResourceDto | void> | undefined;
    try {
      response = await this._httpClient.delete<void>(uri, options).toPromise();
    }
    catch (e) {
      if (e instanceof HttpErrorResponse)
        response = HalClient.convertErrorResponse(e);
      else
        throw new Error(`DELETE ${uri} - options: ${JSON.stringify(options)} failed with error ${e}`);
    }
    if (!response)
      throw new Error(`DELETE ${uri} - options: ${JSON.stringify(options)} did not return a response.`);
    if (!response.ok) {
      const errorResponse = HalClient.convertResponse<TError>(TError, response as HttpResponse<ResourceDto>);
      return errorResponse;
    }

    return response as HttpResponse<void>;
  }

  private static createOptions(headers?: HttpHeaders): { headers?: HttpHeaders; responseType: 'json'; observe: 'response' } {
    headers?.append('Accept', 'application/hal+json')
    return {
      headers: headers,
      responseType: 'json',
      observe: 'response'
    }
  }

  public static convertResponse<TResource extends Resource>(TResource: { new(dto?: ResourceDto | null): TResource }, response: HttpResponse<ResourceDto>): HttpResponse<TResource> {
    const resource = new TResource(response.body);
    const resourceResponse = response.clone<TResource>({ body: resource });
    return resourceResponse;
  }

  private static convertErrorResponse<TResourceDto>(e: HttpErrorResponse): HttpResponse<TResourceDto> {
    const dtoResponse = new HttpResponse({ body: e.error, headers: e.headers, status: e.status, statusText: e.statusText, url: e.url ? e.url : undefined });
    return dtoResponse;
  }
}
