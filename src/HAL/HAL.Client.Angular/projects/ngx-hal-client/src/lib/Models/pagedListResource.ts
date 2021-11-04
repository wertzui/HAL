import { Link, LinkDto } from "./link";
import { ListResource, ListResourceDto } from "./listResource";
import { Page } from "./page";
import { Resource, ResourceDto } from "./resource";

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

export class PagedListResource extends ListResource implements Page {
  _links!: {
    [name: string]: Link[] | undefined;
    self: Link[];
    first?: Link[];
    prev?: Link[];
    next?: Link[];
    last?: Link[];
  };

  currentPage?: number;
  totalPages?: number;

  public static fromDto<TListDto extends ResourceDto>(dto: PagedListResourceDto<TListDto>): PagedListResource {
    const resource = ListResource.fromDto(dto);

    return resource;
  }
}