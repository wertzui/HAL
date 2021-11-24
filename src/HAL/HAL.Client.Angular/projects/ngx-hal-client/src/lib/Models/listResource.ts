import { Resource, ResourceDto } from "./resource";

export interface ListResourceDto<TListDto extends ResourceDto> extends ResourceDto {
  _embedded?: {
    [name: string]: ResourceDto[];
    items: TListDto[];
  };
}

export class ListResource extends Resource {
  _embedded!: {
    [name: string]: Resource[];
    items: Resource[];
  };

  public constructor(dto?: ListResourceDto<any>) {
    super(dto);

    if (!ListResource.isListResource(this))
      throw new TypeError(`The resource ${dto} is not a ListResource.`);
  }

  public static isListResource(resource: ListResource | Resource): resource is ListResource {
    return !!((resource as ListResource)?._embedded?.items)
  }
}