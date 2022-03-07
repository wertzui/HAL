import * as i1 from '@angular/common/http';
import { HttpErrorResponse, HttpResponse } from '@angular/common/http';
import * as i0 from '@angular/core';
import { Injectable, NgModule } from '@angular/core';
import { UriTemplate } from 'uri-templates-es';
import * as _ from 'lodash';

class HalClient {
    constructor(_httpClient) {
        this._httpClient = _httpClient;
    }
    async get(uri, TResource, TError, headers) {
        const options = HalClient.createOptions(headers);
        let dtoResponse;
        try {
            dtoResponse = await this._httpClient.get(uri, options).toPromise();
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
    }
    async post(uri, body, TResource, TError, headers) {
        const options = HalClient.createOptions(headers);
        let dtoResponse;
        try {
            dtoResponse = await this._httpClient.post(uri, body, options).toPromise();
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
    }
    async put(uri, body, TResource, TError, headers) {
        const options = HalClient.createOptions(headers);
        let dtoResponse;
        try {
            dtoResponse = await this._httpClient.put(uri, body, options).toPromise();
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
    }
    async delete(uri, TError, headers) {
        const options = HalClient.createOptions(headers);
        let response;
        try {
            response = await this._httpClient.delete(uri, options).toPromise();
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
    }
    static createOptions(headers) {
        headers?.append('Accept', 'application/hal+json');
        return {
            headers: headers,
            responseType: 'json',
            observe: 'response'
        };
    }
    static convertResponse(TResource, response) {
        const resource = new TResource(response.body);
        const resourceResponse = response.clone({ body: resource });
        return resourceResponse;
    }
    static convertErrorResponse(e) {
        const dtoResponse = new HttpResponse({ body: e.error, headers: e.headers, status: e.status, statusText: e.statusText, url: e.url ? e.url : undefined });
        return dtoResponse;
    }
}
HalClient.ɵfac = i0.ɵɵngDeclareFactory({ minVersion: "12.0.0", version: "13.2.5", ngImport: i0, type: HalClient, deps: [{ token: i1.HttpClient }], target: i0.ɵɵFactoryTarget.Injectable });
HalClient.ɵprov = i0.ɵɵngDeclareInjectable({ minVersion: "12.0.0", version: "13.2.5", ngImport: i0, type: HalClient, providedIn: 'root' });
i0.ɵɵngDeclareClassMetadata({ minVersion: "12.0.0", version: "13.2.5", ngImport: i0, type: HalClient, decorators: [{
            type: Injectable,
            args: [{
                    providedIn: 'root'
                }]
        }], ctorParameters: function () { return [{ type: i1.HttpClient }]; } });

class HalClientModule {
}
HalClientModule.ɵfac = i0.ɵɵngDeclareFactory({ minVersion: "12.0.0", version: "13.2.5", ngImport: i0, type: HalClientModule, deps: [], target: i0.ɵɵFactoryTarget.NgModule });
HalClientModule.ɵmod = i0.ɵɵngDeclareNgModule({ minVersion: "12.0.0", version: "13.2.5", ngImport: i0, type: HalClientModule });
HalClientModule.ɵinj = i0.ɵɵngDeclareInjector({ minVersion: "12.0.0", version: "13.2.5", ngImport: i0, type: HalClientModule, providers: [
        HalClient
    ], imports: [[]] });
i0.ɵɵngDeclareClassMetadata({ minVersion: "12.0.0", version: "13.2.5", ngImport: i0, type: HalClientModule, decorators: [{
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

/**
 *  A Link Object represents a hyperlink from the containing resource to a URI.
 */
class Link {
    fillTemplate(parameters) {
        return new UriTemplate(this.href).fill(parameters);
    }
    static fromDto(dto) {
        const link = Object.assign(new Link(), dto);
        return link;
    }
    static fromDtos(dtos) {
        if (!dtos)
            return [];
        const links = dtos
            .filter(dto => !!dto?.href)
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
    constructor(dto) {
        const links = !(dto?._links) ? {} : Object.fromEntries(Object.entries(dto._links).map(([rel, links]) => [rel, Link.fromDtos(links)]));
        if (!links['self'])
            throw new Error(`The self link is missing in the given ResourceDto: ${JSON.stringify(dto)}`);
        const embedded = !(dto?._embedded) ? {} : Object.fromEntries(Object.entries(dto._embedded).map(([rel, resources]) => [rel, Resource.fromDtos(resources)]));
        const dtoWithParsedDates = Resource.parseDates(dto);
        Object.assign(this, dtoWithParsedDates);
        // We ensured that it has a self property
        this._links = links;
        this._embedded = embedded;
    }
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
    getFormLinkHrefs() {
        const allLinks = this._links;
        if (!allLinks)
            return [];
        return Object.keys(allLinks)
            .filter(key => Resource.isUrl(key));
    }
    static isUrl(possibleUrl) {
        try {
            new URL(possibleUrl);
            return true;
        }
        catch {
            return false;
        }
    }
    //public static fromDto(dto: ResourceDto): Resource;
    //public static fromDto<TResource extends Resource>(dto: ResourceDto, TResource: { new(dto: ResourceDto): TResource }): TResource;
    static fromDto(dto, TResource) {
        const links = !(dto?._links) ? {} : Object.fromEntries(Object.entries(dto._links).map(([rel, links]) => [rel, Link.fromDtos(links)]));
        const embedded = !(dto?._embedded) ? {} : Object.fromEntries(Object.entries(dto._embedded).map(([rel, embeddedResourceDtos]) => [rel, Resource.fromDtos(embeddedResourceDtos, TResource)]));
        const dtoWithParsedDates = Resource.parseDates(dto);
        const resource = Object.assign(TResource ? new TResource(dto) : new Resource(dto), dtoWithParsedDates, { _embedded: embedded, _links: links });
        return resource;
    }
    static fromDtos(dtos, TResource) {
        if (!dtos)
            return [];
        const resources = dtos
            .filter(dto => !!dto)
            .map(dto => Resource.fromDto(dto, TResource));
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
            for (const [key, value] of Object.entries(dto)) {
                dto[key] = this.parseDates(value);
            }
        }
        return dto;
    }
}
Resource._iso8601RegEx = /^([+-]?\d{4}(?!\d{2}\b))((-?)((0[1-9]|1[0-2])(\3([12]\d|0[1-9]|3[01]))?|W([0-4]\d|5[0-2])(-?[1-7])?|(00[1-9]|0[1-9]\d|[12]\d{2}|3([0-5]\d|6[1-6])))([T\s]((([01]\d|2[0-3])((:?)[0-5]\d)?|24:?00)([.,]\d+(?!:))?)?(\17[0-5]\d([.,]\d+)?)?([zZ]|([+-])([01]\d|2[0-3]):?([0-5]\d)?)?)?)?$/;

;
var PropertyType;
(function (PropertyType) {
    PropertyType["Hidden"] = "hidden";
    PropertyType["Text"] = "text";
    PropertyType["Textarea"] = "textarea";
    PropertyType["Search"] = "search";
    PropertyType["Tel"] = "tel";
    PropertyType["Url"] = "url";
    PropertyType["Email"] = "email";
    PropertyType["Password"] = "password";
    PropertyType["Date"] = "date";
    PropertyType["Month"] = "month";
    PropertyType["Week"] = "week";
    PropertyType["Time"] = "time";
    PropertyType["DatetimeLocal"] = "datetime-local";
    PropertyType["Number"] = "number";
    PropertyType["Range"] = "range";
    PropertyType["Color"] = "color";
    PropertyType["Bool"] = "bool";
    PropertyType["DatetimeOffset"] = "datetime-offset";
    PropertyType["Duration"] = "duration";
    PropertyType["Image"] = "image";
    PropertyType["File"] = "file";
    PropertyType["Collection"] = "collection";
    PropertyType["Object"] = "object";
})(PropertyType || (PropertyType = {}));
class Options {
    constructor(dto) {
        this.inline = [];
        Object.assign(this, dto);
        if (!this.inline)
            this.inline = [];
    }
}
class Property {
    constructor(dto) {
        Object.assign(this, dto);
        this._templates = !(dto?._templates) ? {} : Object.fromEntries(Object.entries(dto._templates).map(([rel, templateDto]) => [rel, new Template(templateDto)]));
        if (this.options)
            this.options = new Options(dto?.options);
    }
}
;
class Template {
    constructor(dto) {
        Object.assign(this, dto);
        this.properties = !(dto?.properties) ? [] : dto.properties.map(propertyDto => new Property(propertyDto));
    }
    get values() {
        return !this.properties ? {} : Object.fromEntries(this.properties.map(property => [property.name, property.value]));
    }
}
class FormsResource extends Resource {
    constructor(dto) {
        super(dto);
        this._templates = !(dto?._templates) ? {} : Object.fromEntries(Object.entries(dto._templates).map(([rel, templateDto]) => [rel, new Template(templateDto)]));
    }
    getTemplate(name) {
        const templateNames = Object.getOwnPropertyNames(this._templates);
        if (!templateNames.includes(name))
            throw new Error(`The form ${this} does not have a _template with the name '${name}'. It only has ${templateNames}.`);
        const template = this._templates[name];
        return template;
    }
}

class ListResource extends Resource {
    constructor(dto) {
        super(dto);
        if (!ListResource.isListResource(this))
            throw new TypeError(`The resource ${dto} is not a ListResource.`);
    }
    static isListResource(resource) {
        return !!(resource?._embedded?.items);
    }
}

class PagedListResource extends ListResource {
    constructor(dto) {
        super(dto);
    }
}

/*
 * Public API Surface of ngx-hal-client
 */

/**
 * Generated bundle index. Do not edit.
 */

export { FormsResource, HalClient, HalClientModule, Link, ListResource, Options, PagedListResource, Property, PropertyType, Resource, Template };
//# sourceMappingURL=wertzui-ngx-hal-client.mjs.map
