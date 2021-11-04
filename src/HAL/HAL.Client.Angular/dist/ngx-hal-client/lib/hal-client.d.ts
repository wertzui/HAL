import { HttpClient, HttpHeaders, HttpResponse } from '@angular/common/http';
import { Resource, ResourceDto } from './Models/resource';
import * as i0 from "@angular/core";
export declare class HalClient {
    private _httpClient;
    constructor(_httpClient: HttpClient);
    get<TResource extends Resource, TError extends Resource>(uri: string, TResource: {
        new (): TResource;
    }, TError: {
        new (): TError;
    }, headers?: HttpHeaders): Promise<HttpResponse<TResource | TError>>;
    post<TResource extends Resource, TError extends Resource>(uri: string, body: Resource, TResource: {
        new (): TResource;
    }, TError: {
        new (): TError;
    }, headers?: HttpHeaders): Promise<HttpResponse<TResource | TError>>;
    put<TResource extends Resource, TError extends Resource>(uri: string, body: Resource, TResource: {
        new (): TResource;
    }, TError: {
        new (): TError;
    }, headers?: HttpHeaders): Promise<HttpResponse<TResource | TError>>;
    delete<TError extends Resource>(uri: string, TError: {
        new (): TError;
    }, headers?: HttpHeaders): Promise<HttpResponse<void | TError>>;
    private static createOptions;
    static convertResponse<TResource extends Resource>(TResource: {
        new (): TResource;
    }, response: HttpResponse<ResourceDto>): HttpResponse<TResource>;
    private static convertErrorResponse;
    static ɵfac: i0.ɵɵFactoryDeclaration<HalClient, never>;
    static ɵprov: i0.ɵɵInjectableDeclaration<HalClient>;
}
