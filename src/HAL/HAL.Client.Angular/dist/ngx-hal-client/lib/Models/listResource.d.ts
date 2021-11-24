import { Resource, ResourceDto } from "./resource";
export interface ListResourceDto<TListDto extends ResourceDto> extends ResourceDto {
    _embedded?: {
        [name: string]: ResourceDto[];
        items: TListDto[];
    };
}
export declare class ListResource extends Resource {
    _embedded: {
        [name: string]: Resource[];
        items: Resource[];
    };
    constructor(dto?: ListResourceDto<any>);
    static isListResource(resource: ListResource | Resource): resource is ListResource;
}
