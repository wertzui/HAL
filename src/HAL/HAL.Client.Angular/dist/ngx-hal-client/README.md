# Ngx-HAL-Client
This package provides a client that can easily access any backend that serves `application/hal+json` resources.
Apart from the client itself, it provides `Resource` and `Link` classes which will automatically be returned if your servers response is a valid HAL-resource.

# Usage
Simply inject it into your components, there is no need to import it into your module.
The ProblemDetails is just what an ASP.Net Core backend will serve in case of an error. If you backend serves something else, simply adjust the interface and the class.
DTO is always whatever your backend returns minus the `_link` and `_embedded` properties. The interface is mostly there for static type checking.
The class however should validate that whatever you think, your DTO is, is really what your backend returned.

```
export class RESTworldClient {

  private _homeUri = 'https://localhost:1234';

  constructor(
    private _halClient: HalClient
  ) { }

  public async getFoo() : Promise<FooResource> {
      const homeResource = await this._halClient.get(this._homeUri, Resource, ProblemDetails);
      if(ProblemDetails.isProblemDetails(homeResource)) {
          throw new Error(homeResource);
      }

      const fooLink = homeResource.findLink('foo');
      const fooResource = await this._halClient.get(fooLink.href, FooResource, ProblemDetails);
      if(ProblemDetails.isProblemDetails(fooResource)) {
          throw new Error(homeResource);
      }

      return fooResource;
  }

}

export interface FooDto {
    public foo: string;
}

export class FooResource extends Resource implements ResourceOfDto<FooDto> {
    public foo: string;
}

/*
 * ProblemDetails is what an ASP.Net Core backend returns in case of an error.
 * */
export interface ProblemDetailsDto {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  extensions?: { [key: string] : any }
}

export class ProblemDetails extends Resource implements ResourceOfDto<ProblemDetailsDto> {
  public type?: string;
  public title?: string;
  public status?: number;
  public detail?: string;
  public instance?: string;
  public extensions?: { [key: string]: any }

  public static isProblemDetails(resource: any): resource is ProblemDetails {
    return resource instanceof ProblemDetails;
  }

  public static containsProblemDetailsInformation(resource: any) {
    return resource && (resource instanceof ProblemDetails || (resource instanceof Resource && resource.hasOwnProperty('status') && _.isNumber((<any>resource).status) && (<any>resource).status >= 100 && (<any>resource).status < 600));
  }

  public static fromResource(resource: Resource | null | undefined): ProblemDetails {
    if (!ProblemDetails.containsProblemDetailsInformation(resource))
      throw new Error(`The resource ${resource} does not have problem details.`);

    return Object.assign(new ProblemDetails(), resource);
  }
}
```
