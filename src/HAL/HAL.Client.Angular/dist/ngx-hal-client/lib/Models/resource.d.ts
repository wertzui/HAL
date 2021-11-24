import { Link, LinkDto } from "./link";
/**
 *  A Resource Object represents a resource.
 *  It has two reserved properties:
 *  (1)  "_links": contains links to other resources.
 *  (2)  "_embedded": contains embedded resources.
 */
export interface ResourceDto {
    /**
     *  The reserved "_embedded" property is OPTIONAL
     *  It is an object whose property names are link relation types (as
     *  defined by [RFC5988]) and values are either a Resource Object or an
     *  array of Resource Objects.   Embedded Resources MAY be a full, partial,
     *  or inconsistent version of   the representation served from the target URI.
     */
    _embedded?: {
        [name: string]: ResourceDto[];
    };
    /**
     *  The reserved "_links" property is OPTIONAL.
     *  It is an object whose property names are link relation types (as
     *  defined by [RFC5988]) and values are either a Link Object or an array
     *  of Link Objects.  The subject resource of these links is the Resource
     *  Object of which the containing "_links" object is a property.
     */
    _links?: {
        [name: string]: LinkDto[] | null | undefined;
        self: LinkDto[];
    };
}
/**
 *  A Resource Object represents a resource.
 *  It has two reserved properties:
 *  (1)  "_links": contains links to other resources.
 *  (2)  "_embedded": contains embedded resources.
 */
export declare class Resource {
    private static _iso8601RegEx;
    /**
     *  The reserved "_embedded" property is OPTIONAL
     *  It is an object whose property names are link relation types (as
     *  defined by [RFC5988]) and values are either a Resource Object or an
     *  array of Resource Objects.   Embedded Resources MAY be a full, partial,
     *  or inconsistent version of   the representation served from the target URI.
     */
    _embedded: {
        [name: string]: Resource[];
    };
    /**
     *  The reserved "_links" property is OPTIONAL.
     *  It is an object whose property names are link relation types (as
     *  defined by [RFC5988]) and values are either a Link Object or an array
     *  of Link Objects.  The subject resource of these links is the Resource
     *  Object of which the containing "_links" object is a property.
     */
    _links: {
        [name: string]: Link[] | undefined;
        self: Link[];
    };
    constructor(dto?: ResourceDto);
    findLinks(rel: string): Link[];
    findLink(rel: string, name?: string): Link | undefined;
    findEmbedded(rel: string): Resource[];
    getFormLinkHrefs(): string[];
    private static isUrl;
    private static fromDto;
    private static fromDtos;
    private static parseDates;
}
