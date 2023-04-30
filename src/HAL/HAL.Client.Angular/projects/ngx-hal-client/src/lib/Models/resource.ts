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
  private static readonly _iso8601RegEx = /^([+-]?\d{4}(?!\d{2}\b))((-)((0[1-9]|1[0-2])(\3([12]\d|0[1-9]|3[01]))?|W([0-4]\d|5[0-2])(-?[1-7])?|(00[1-9]|0[1-9]\d|[12]\d{2}|3([0-5]\d|6[1-6])))([T\s]((([01]\d|2[0-3])((:?)[0-5]\d)?|24:?00)([.,]\d+(?!:))?)?(\17[0-5]\d([.,]\d+)?)?([zZ]|([+-])([01]\d|2[0-3]):?([0-5]\d)?)?)?)$/;
  private static readonly _timeRegEx = /^\d{1,2}:\d{1,2}(?::\d{1,2}(?:.\d+)?)?$/;

  /** 
   *  The reserved "_embedded" property is OPTIONAL   
   *  It is an object whose property names are link relation types (as   
   *  defined by [RFC5988]) and values are either a Resource Object or an   
   *  array of Resource Objects.   Embedded Resources MAY be a full, partial, 
   *  or inconsistent version of   the representation served from the target URI. 
   */
  public _embedded: { [name: string]: Resource[] };
  /** 
   *  The reserved "_links" property is OPTIONAL.   
   *  It is an object whose property names are link relation types (as   
   *  defined by [RFC5988]) and values are either a Link Object or an array   
   *  of Link Objects.  The subject resource of these links is the Resource   
   *  Object of which the containing "_links" object is a property. 
   */
  public _links: {
    [name: string]: Link[] | undefined;
    self: Link[];
  };

  public constructor(dto?: ResourceDto) {
    const links = !(dto?._links) ? {} : Object.fromEntries(Object.entries(dto._links).map(([rel, links]) => [rel, Link.fromDtos(links)]));
    if (!links['self'])
      throw new Error(`The self link is missing in the given ResourceDto: ${JSON.stringify(dto)}`);

    const embedded = !(dto?._embedded) ? {} : Object.fromEntries(Object.entries(dto._embedded).map(([rel, resources]) => [rel, Resource.fromDtos(resources)]));
    const dtoWithParsedDates = Resource.parseDates(dto);

    Object.assign(this, dtoWithParsedDates);

    // We ensured that it has a self property
    this._links = links as {
      [name: string]: Link[] | undefined;
      self: Link[];
    };
    this._embedded = embedded;
  }

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

  public getFormLinkHrefs(): string[] {
    const allLinks = this._links;

    if (!allLinks)
      return [];

    return Object.keys(allLinks)
      .filter(key => Resource.isUrl(key));
  }

  private static isUrl(possibleUrl: string): boolean {
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
  private static fromDto<TResource extends Resource>(dto: ResourceDto, TResource?: { new(dto: ResourceDto): TResource }): TResource | Resource {
    const links = !(dto?._links) ? {} : Object.fromEntries(Object.entries(dto._links).map(([rel, links]) => [rel, Link.fromDtos(links)]));
    const embedded = !(dto?._embedded) ? {} : Object.fromEntries(Object.entries(dto._embedded).map(([rel, embeddedResourceDtos]) => [rel, Resource.fromDtos(embeddedResourceDtos, TResource)]));
    const dtoWithParsedDates = Resource.parseDates(dto);

    const resource = Object.assign(TResource ? new TResource(dto) : new Resource(dto), dtoWithParsedDates, { _embedded: embedded, _links: links });

    return resource;
  }

  private static fromDtos<TResource extends Resource>(dtos: ResourceDto[] | null | undefined, TResource?: { new(dto: ResourceDto): TResource }): (TResource | Resource)[] {
    if (!dtos)
      return [];

    const resources = dtos
      .filter(dto => !!dto)
      .map(dto => Resource.fromDto(dto, TResource));

    return resources;
  }

  private static parseDates(dto: unknown): unknown {
    if (dto === null || dto === undefined)
      return dto;

    if (_.isString(dto)) {
      if (this._iso8601RegEx.test(dto))
        return new Date(dto);
      else if (this._timeRegEx.test(dto))
        return new Date("0001-01-01T" + dto);
    }

    else if (_.isArray(dto)) {
      for (let i = 0; i < dto.length; i++) {
        dto[i] = this.parseDates(dto[i]);
      }
    }

    else if (_.isPlainObject(dto)) {
      for (const [key, value] of Object.entries(dto as { [name: string]: unknown })) {
        (dto as { [name: string]: unknown })[key] = this.parseDates(value);
      }
    }

    return dto;
  }
}
