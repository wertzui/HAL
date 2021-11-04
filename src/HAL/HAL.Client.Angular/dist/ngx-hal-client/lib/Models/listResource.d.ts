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
    static fromDto<TListDto extends ResourceDto>(dto: ListResourceDto<TListDto>): ListResource;
    static isListResource(resource: ListResource | Resource): resource is ListResource;
}
