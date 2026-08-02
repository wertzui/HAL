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

  /**
   * Creates a new Resource instance from the given ResourceDto.
   * @param dto The ResourceDto to create the Resource instance from.
   * @throws An error if the self link is missing in the given ResourceDto.
   */
  public constructor(dto: ResourceDto) {
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

  /**
   * Finds all links with the given relation type.
   * @param rel The relation type to search for.
   * @returns An array of links with the given relation type, or an empty array if none are found.
   */
  public findLinks(rel: string): Link[] {
    const linksWithRel = this._links[rel];

    if (!linksWithRel)
      return [];

    return linksWithRel;
  }

  /**
   * Finds a link with the specified relationship and optional name.
   * @param rel The relationship of the link to find.
   * @param name (Optional) The name of the link to find.
   * @returns The link with the specified relationship and name, or undefined if not found.
   */
  public findLink(rel: string, name?: string): Link | undefined {
    const linksWithRel = this.findLinks(rel);

    if (linksWithRel.length === 0)
      return undefined;

    if (name)
      return linksWithRel.find(link => link.name === name);

    return linksWithRel[0];
  }

  /**
   * Finds and returns an array of embedded resources with the specified relation type.
   * @param rel The relation type to search for.
   * @returns An array of embedded resources with the specified relation type, or an empty array if none are found.
   */
  public findEmbedded(rel: string): Resource[] {
    const embeddedWithRel = this._embedded[rel];

    if (!embeddedWithRel)
      return [];

    return embeddedWithRel;
  }

  /**
   * Returns an array of hrefs for all form links in the resource.
   * Form links are links where the relation is a valid URL.
   * @returns An array of hrefs for all form links in the resource.
   */
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

  private static fromDto<TResource extends Resource>(dto: ResourceDto, TResource?: { new(dto: ResourceDto): TResource }): TResource | Resource {
    return TResource ? new TResource(dto) : new Resource(dto);
  }

  private static fromDtos<TResource extends Resource>(dtos: ResourceDto[] | null | undefined, TResource?: { new(dto: ResourceDto): TResource }): (TResource | Resource)[] {
    if (!dtos)
      return [];

    const resources = dtos
      .filter(dto => !!dto)
      .map(dto => Resource.fromDto(dto, TResource));

    return resources;
  }

  private static parseDates<T>(dto: T): T | Date {
    if (dto === null || dto === undefined)
      return dto;

    if (typeof dto === "string") {
      if (this._iso8601RegEx.test(dto)) {
        const maybeDate = new Date(dto);
        if (!isNaN(maybeDate.getTime()))
          return maybeDate;
      }

      if (this._timeRegEx.test(dto)) {
        const maybeTime = new Date("0001-01-01T" + dto);
        if (!isNaN(maybeTime.getTime()))
          return maybeTime;
      }

      return dto;
    }

    if (Array.isArray(dto)) {
      // Build a new array instead of mutating the one that was passed in.
      return dto.map(item => this.parseDates(item)) as unknown as T;
    }

    if (typeof dto === "object") {
      // Build a new object instead of mutating the one that was passed in.
      const result: { [name: string]: unknown } = {};

      for (const [key, value] of Object.entries(dto as { [name: string]: unknown })) {
        // _links and _embedded are already handled separately by the constructor (via
        // Link.fromDtos and recursive Resource construction, respectively), which will parse
        // dates for embedded resources itself. Recursing into them here as well would visit
        // every embedded resource dto multiple times for no benefit, since the result is
        // discarded in favor of the separately computed _links/_embedded values anyway.
        if (key === "_links" || key === "_embedded")
          continue;

        result[key] = this.parseDates(value);
      }

      return result as T;
    }

    return dto;
  }
}
