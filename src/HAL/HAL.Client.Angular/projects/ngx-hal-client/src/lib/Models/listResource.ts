import { Resource, ResourceDto } from "./resource";

export interface ListResourceDto<TListDto extends ResourceDto> extends ResourceDto {
  _embedded?: {
    [name: string]: ResourceDto[];
    items: TListDto[]
  };
}

export class ListResource extends Resource {
  _embedded!: {
    [name: string]: Resource[];
    items: Resource[];
  };

  public static fromDto<TListDto extends ResourceDto>(dto: ListResourceDto<TListDto>): ListResource {
    const resource = Resource.fromDto(dto);

    if (!ListResource.isListResource(resource)) {
      throw new TypeError(`The resource ${resource} is not a ListResource.`);
    }

    return resource;
  }

  public static isListResource(resource: ListResource | Resource): resource is ListResource {
    return !!((resource as ListResource)?._embedded?.items)
  }
}