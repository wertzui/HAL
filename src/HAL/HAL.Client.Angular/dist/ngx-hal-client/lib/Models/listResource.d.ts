import { Resource, ResourceDto } from "./resource";
import { ResourceOfDto } from './resourceOf';
export interface ListResourceDto<TListDto extends ResourceDto> extends ResourceDto {
    _embedded?: {
        [name: string]: ResourceDto[];
        items: TListDto[];
    };
}
export declare class ListResource<TListDto extends ResourceDto> extends Resource {
    _embedded: {
        [name: string]: Resource[];
        items: ResourceOfDto<TListDto>[];
    };
    constructor(dto?: ListResourceDto<TListDto>);
}
