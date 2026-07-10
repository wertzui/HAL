import { Link, LinkDto } from "./link";
import { ListResource, ListResourceDto } from "./listResource";
import { Page } from "./page";
import { ResourceDto } from "./resource";


/**
 * Represents a paged list of resources.
 * @template TListDto The type of the resource in the list.
 */
export interface PagedListResourceDto<TListDto> extends ListResourceDto<TListDto>, Page {
  _links?: {
    [name: string]: LinkDto[] | null | undefined;
    self: LinkDto[];
    first?: LinkDto[];
    prev?: LinkDto[];
    next?: LinkDto[];
    last?: LinkDto[];
  };
}

/**
 * Represents a paged list of resources.
 * @template TListDto The type of the resource in the list.
 */
export class PagedListResource<TListDto> extends ListResource<TListDto> implements Page {
  public declare _links: {
    [name: string]: Link[] | undefined;
    self: Link[];
    first?: Link[];
    prev?: Link[];
    next?: Link[];
    last?: Link[];
  };

  public declare currentPage?: number;
  public declare totalPages?: number;

  public constructor(dto: PagedListResourceDto<TListDto>) {
    super(dto);
  }
}