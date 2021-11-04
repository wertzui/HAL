import { Link, LinkDto } from "./link";
import * as _ from 'lodash';

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
  _embedded?: { [name: string]: ResourceDto[] };
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
export class Resource {
  private static _iso8601RegEx: RegExp = /^(\d{4})-(\d{2})-(\d{2})T(\d{2}):(\d{2}):(\d{2}(?:\.\d*))(?:Z|(\+|-)([\d|:]*))?$/;
  /** 
   *  The reserved "_embedded" property is OPTIONAL   
   *  It is an object whose property names are link relation types (as   
   *  defined by [RFC5988]) and values are either a Resource Object or an   
   *  array of Resource Objects.   Embedded Resources MAY be a full, partial, 
   *  or inconsistent version of   the representation served from the target URI. 
   */
  public _embedded!: { [name: string]: Resource[] };
  /** 
   *  The reserved "_links" property is OPTIONAL.   
   *  It is an object whose property names are link relation types (as   
   *  defined by [RFC5988]) and values are either a Link Object or an array   
   *  of Link Objects.  The subject resource of these links is the Resource   
   *  Object of which the containing "_links" object is a property. 
   */
  public _links!: {
    [name: string]: Link[] | undefined;
    self: Link[];
  };

  public findLinks(rel: string): Link[] {
    const linksWithRel = this._links[rel];

    if (!linksWithRel)
      return [];

    return linksWithRel;
  }

  public findLink(rel: string, name?: string): Link | undefined {
    const linksWithRel = this.findLinks(rel);

    if (linksWithRel.length === 0)
      return undefined;

    if (name)
      return linksWithRel.find(link => link.name === name);

    return linksWithRel[0];
  }

  public findEmbedded(rel: string): Resource[] {
    const embeddedWithRel = this._embedded[rel];

    if (!embeddedWithRel)
      return [];

    return embeddedWithRel;
  }

  public static fromDto(dto: ResourceDto): Resource;
  public static fromDto<TResource extends Resource>(dto: ResourceDto, TResource: { new(): TResource }): TResource;
  public static fromDto<TResource extends Resource>(dto: ResourceDto, TResource?: { new(): TResource }): TResource | Resource {
    const links = !(dto?._links) ? {} : Object.fromEntries(Object.entries(dto._links).map(([rel, links]) => [rel, Link.fromDtos(links)]));
    const embedded = !(dto?._embedded) ? {} : Object.fromEntries(Object.entries(dto._embedded).map(([rel, resources]) => [rel, Resource.fromDtos(resources)]));
    const dtoWithParsedDates = Resource.parseDates(dto);

    const resource = Object.assign(!!(TResource) ? new TResource() : new Resource(), dtoWithParsedDates, { _embedded: embedded, _links: links });

    return resource;
  }

  public static fromDtos(dtos: ResourceDto[] | null | undefined): Resource[] {
    if (!dtos)
      return [];

    const resources = dtos
      .filter(dto => !!dto)
      .map(dto => Resource.fromDto(dto));

    return resources;
  }

  private static parseDates(dto: any): any {
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
