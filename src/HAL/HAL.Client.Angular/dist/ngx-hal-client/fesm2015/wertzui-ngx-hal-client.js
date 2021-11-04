import { __awaiter } from 'tslib';
import * as i1 from '@angular/common/http';
import { HttpErrorResponse, HttpResponse } from '@angular/common/http';
import * as i0 from '@angular/core';
import { Injectable, NgModule } from '@angular/core';
import * as utpl from 'uri-templates';
import * as _ from 'lodash';

/**
 *  A Link Object represents a hyperlink from the containing resource to a URI.
 */
class Link {
    fillTemplate(parameters) {
        return utpl(this.href).fill(parameters);
    }
    static fromDto(dto) {
        const link = Object.assign(new Link(), dto);
        return link;
    }
    static fromDtos(dtos) {
        if (!dtos)
            return [];
        const links = dtos
            .filter(dto => !!(dto === null || dto === void 0 ? void 0 : dto.href))
            .map(dto => Link.fromDto(dto));
        return links;
    }
}

/**
 *  A Resource Object represents a resource.
 *  It has two reserved properties:
 *  (1)  "_links": contains links to other resources.
 *  (2)  "_embedded": contains embedded resources.
 */
class Resource {
    findLinks(rel) {
        const linksWithRel = this._links[rel];
        if (!linksWithRel)
            return [];
        return linksWithRel;
    }
    findLink(rel, name) {
        const linksWithRel = this.findLinks(rel);
        if (linksWithRel.length === 0)
            return undefined;
        if (name)
            return linksWithRel.find(link => link.name === name);
        return linksWithRel[0];
    }
    findEmbedded(rel) {
        const embeddedWithRel = this._embedded[rel];
        if (!embeddedWithRel)
            return [];
        return embeddedWithRel;
    }
    static fromDto(dto, TResource) {
        const links = !(dto === null || dto === void 0 ? void 0 : dto._links) ? {} : Object.fromEntries(Object.entries(dto._links).map(([rel, links]) => [rel, Link.fromDtos(links)]));
        const embedded = !(dto === null || dto === void 0 ? void 0 : dto._embedded) ? {} : Object.fromEntries(Object.entries(dto._embedded).map(([rel, resources]) => [rel, Resource.fromDtos(resources)]));
        const dtoWithParsedDates = Resource.parseDates(dto);
        const resource = Object.assign(!!(TResource) ? new TResource() : new Resource(), dtoWithParsedDates, { _embedded: embedded, _links: links });
        return resource;
    }
    static fromDtos(dtos) {
        if (!dtos)
            return [];
        const resources = dtos
            .filter(dto => !!dto)
            .map(dto => Resource.fromDto(dto));
        return resources;
    }
    static parseDates(dto) {
        if (dto === null || dto === undefined)
            return dto;
        if (_.isString(dto)) {
            if (this._iso8601RegEx.test(dto))
                return new Date(dto);
        }
        else if (_.isArray(dto)) {
            for (let i = 0; i < dto.length; i++) {
                dto[i] = this.parseDates(dto[i]);
            }
        }
        else if (_.isPlainObject(dto)) {
            for (let [key, value] of Object.entries(dto)) {
                dto[key] = this.parseDates(value);
            }
        }
        return dto;
    }
}
Resource._iso8601RegEx = /^(\d{4})-(\d{2})-(\d{2})T(\d{2}):(\d{2}):(\d{2}(?:\.\d*))(?:Z|(\+|-)([\d|:]*))?$/;

class HalClient {
    constructor(_httpClient) {
        this._httpClient = _httpClient;
    }
    get(uri, TResource, TError, headers) {
        return __awaiter(this, void 0, void 0, function* () {
            const options = HalClient.createOptions(headers);
            let dtoResponse;
            try {
                dtoResponse = yield this._httpClient.get(uri, options).toPromise();
            }
            catch (e) {
                if (e instanceof HttpErrorResponse)
                    dtoResponse = HalClient.convertErrorResponse(e);
                else
                    throw new Error(`GET ${uri} - options: ${options} failed with error ${e}`);
            }
            if (!dtoResponse)
                throw new Error(`GET ${uri} - options: ${options} did not return a response.`);
            const resourceResponse = HalClient.convertResponse(dtoResponse.ok ? TResource : TError, dtoResponse);
            return resourceResponse;
        });
    }
    post(uri, body, TResource, TError, headers) {
        return __awaiter(this, void 0, void 0, function* () {
            const options = HalClient.createOptions(headers);
            let dtoResponse;
            try {
                dtoResponse = yield this._httpClient.post(uri, body, options).toPromise();
            }
            catch (e) {
                if (e instanceof HttpErrorResponse)
                    dtoResponse = HalClient.convertErrorResponse(e);
                else
                    throw new Error(`POST ${uri} - options: ${options} - body: ${body} failed with error ${e}`);
            }
            if (!dtoResponse)
                throw new Error(`POST ${uri} - options: ${options} - body: ${body} did not return a response.`);
            const resourceResponse = HalClient.convertResponse(dtoResponse.ok ? TResource : TError, dtoResponse);
            return resourceResponse;
        });
    }
    put(uri, body, TResource, TError, headers) {
        return __awaiter(this, void 0, void 0, function* () {
            const options = HalClient.createOptions(headers);
            let dtoResponse;
            try {
                dtoResponse = yield this._httpClient.put(uri, body, options).toPromise();
            }
            catch (e) {
                if (e instanceof HttpErrorResponse)
                    dtoResponse = HalClient.convertErrorResponse(e);
                else
                    throw new Error(`PUT ${uri} - options: ${options} - body: ${body} failed with error ${e}`);
            }
            if (!dtoResponse)
                throw new Error(`PUT ${uri} - options: ${options} - body: ${body} did not return a response.`);
            const resourceResponse = HalClient.convertResponse(dtoResponse.ok ? TResource : TError, dtoResponse);
            return resourceResponse;
        });
    }
    delete(uri, TError, headers) {
        return __awaiter(this, void 0, void 0, function* () {
            const options = HalClient.createOptions(headers);
            let response;
            try {
                response = yield this._httpClient.delete(uri, options).toPromise();
            }
            catch (e) {
                if (e instanceof HttpErrorResponse)
                    response = HalClient.convertErrorResponse(e);
                else
                    throw new Error(`DELETE ${uri} - options: ${options} failed with error ${e}`);
            }
            if (!response)
                throw new Error(`DELETE ${uri} - options: ${options} did not return a response.`);
            if (!response.ok) {
                const errorResponse = HalClient.convertResponse(TError, response);
                return errorResponse;
            }
            return response;
        });
    }
    static createOptions(headers) {
        headers === null || headers === void 0 ? void 0 : headers.append('Accept', 'application/hal+json');
        return {
            headers: headers,
            responseType: 'json',
            observe: 'response'
        };
    }
    static convertResponse(TResource, response) {
        const resource = Resource.fromDto(response.body || new TResource(), TResource);
        const resourceResponse = response.clone({ body: resource });
        return resourceResponse;
    }
    static convertErrorResponse(e) {
        const dtoResponse = new HttpResponse({ body: e.error, headers: e.headers, status: e.status, statusText: e.statusText, url: e.url ? e.url : undefined });
        return dtoResponse;
    }
}
HalClient.ɵfac = i0.ɵɵngDeclareFactory({ minVersion: "12.0.0", version: "12.2.11", ngImport: i0, type: HalClient, deps: [{ token: i1.HttpClient }], target: i0.ɵɵFactoryTarget.Injectable });
HalClient.ɵprov = i0.ɵɵngDeclareInjectable({ minVersion: "12.0.0", version: "12.2.11", ngImport: i0, type: HalClient, providedIn: 'root' });
i0.ɵɵngDeclareClassMetadata({ minVersion: "12.0.0", version: "12.2.11", ngImport: i0, type: HalClient, decorators: [{
            type: Injectable,
            args: [{
                    providedIn: 'root'
                }]
        }], ctorParameters: function () { return [{ type: i1.HttpClient }]; } });

class HalClientModule {
}
HalClientModule.ɵfac = i0.ɵɵngDeclareFactory({ minVersion: "12.0.0", version: "12.2.11", ngImport: i0, type: HalClientModule, deps: [], target: i0.ɵɵFactoryTarget.NgModule });
HalClientModule.ɵmod = i0.ɵɵngDeclareNgModule({ minVersion: "12.0.0", version: "12.2.11", ngImport: i0, type: HalClientModule });
HalClientModule.ɵinj = i0.ɵɵngDeclareInjector({ minVersion: "12.0.0", version: "12.2.11", ngImport: i0, type: HalClientModule, providers: [
        HalClient
    ], imports: [[]] });
i0.ɵɵngDeclareClassMetadata({ minVersion: "12.0.0", version: "12.2.11", ngImport: i0, type: HalClientModule, decorators: [{
            type: NgModule,
            args: [{
                    declarations: [],
                    imports: [],
                    exports: [],
                    providers: [
                        HalClient
                    ]
                }]
        }] });

class ListResource extends Resource {
    static fromDto(dto) {
        const resource = Resource.fromDto(dto);
        if (!ListResource.isListResource(resource)) {
            throw new TypeError(`The resource ${resource} is not a ListResource.`);
        }
        return resource;
    }
    static isListResource(resource) {
        var _a, _b;
        return !!((_b = (_a = resource) === null || _a === void 0 ? void 0 : _a._embedded) === null || _b === void 0 ? void 0 : _b.items);
    }
}

class PagedListResource extends ListResource {
    static fromDto(dto) {
        const resource = ListResource.fromDto(dto);
        return resource;
    }
}

/*
 * Public API Surface of ngx-hal-client
 */

/**
 * Generated bundle index. Do not edit.
 */

export { HalClient, HalClientModule, Link, ListResource, PagedListResource, Resource };
//# sourceMappingURL=wertzui-ngx-hal-client.js.map
