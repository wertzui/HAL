import { Link, LinkDto } from "./link";
import { ListResource, ListResourceDto } from "./listResource";
import { Page } from "./page";
import { ResourceDto } from "./resource";

export interface PagedListResourceDto<TListDto extends ResourceDto> extends ListResourceDto<TListDto>, Page {
  _links?: {
    [name: string]: LinkDto[] | null | undefined;
    self: LinkDto[];
    first?: LinkDto[];
    prev?: LinkDto[];
    next?: LinkDto[];
    last?: LinkDto[];
  };
}

export class PagedListResource<TListDto extends ResourceDto> extends ListResource<TListDto> implements Page {
  declare _links: {
    [name: string]: Link[] | undefined;
    self: Link[];
    first?: Link[];
    prev?: Link[];
    next?: Link[];
    last?: Link[];
  };

  currentPage?: number;
  totalPages?: number;

  public constructor(dto?: PagedListResourceDto<TListDto>) {
    super(dto);
  }
}