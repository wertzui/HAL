import { Resource, ResourceDto } from "./resource";
import { ResourceOfDto } from './resourceOf';

export interface ListResourceDto<TListDto extends ResourceDto> extends ResourceDto {
  _embedded?: {
    [name: string]: ResourceDto[];
    items: TListDto[];
  };
}

export class ListResource<TListDto extends ResourceDto> extends Resource {
  _embedded!: {
    [name: string]: Resource[];
    items: ResourceOfDto<TListDto>[];
  };

  public constructor(dto?: ListResourceDto<TListDto>) {
    super(dto);

    // Ensure that we have a list, even if we got an empty list from our backend.
    if (!this._embedded)
      this._embedded = { items: [] };
    else if (!this._embedded.items)
      this._embedded.items = [];

  }
}