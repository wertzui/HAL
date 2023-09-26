import { Resource, ResourceDto } from "./resource";
import { ResourceOfDto } from './resourceOf';

/**
 * Represents a list resource DTO that contains a list of embedded resources.
 * The embedded resources are stored in the `_embedded.items` property.
 * @template TListDto The type of the embedded resource DTOs.
 */
export interface ListResourceDto<TListDto> extends ResourceDto {
  _embedded?: {
    [name: string]: ResourceDto[];
    items: (TListDto & ResourceDto)[];
  };
}

/**
 * Represents a list resource that contains a list of embedded resources.
 * The embedded resources are stored in the `_embedded.items` property.
 * @template TListDto The type of the embedded resource DTOs.
 */
export class ListResource<TListDto> extends Resource {
  declare _embedded: {
    [name: string]: Resource[];
    items: ResourceOfDto<TListDto>[];
  };

  /**
   * Creates a new ListResource instance from the given DTO.
   * @param dto The DTO for the list resource.
   */
  public constructor(dto: ListResourceDto<TListDto>) {
    super(dto);

    // Ensure that we have a list, even if we got an empty list from our backend.
    if (!this._embedded)
      this._embedded = { items: [] };
    else if (!this._embedded.items)
      this._embedded.items = [];

  }
}