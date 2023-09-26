import { Resource, ResourceDto } from "../models/resource";
import { FormsResource, FormsResourceDto } from "../models/formsResource";
import { ProblemDetails, ProblemDetailsDto } from "../models/problem-details";
import { ListResource, ListResourceDto } from "../models/listResource";
import { PagedListResource, PagedListResourceDto } from "../models/pagedListResource";
import { PagedListFormsResource, PagedListFormsResourceDto } from "../models/pagedListFormsResource";

/**
 * A factory class for creating various types of resources.
 * It is mostly used in conjunction with the HalClient, which will automatically create resources from the DTOs it receives.
 */
export class ResourceFactory {
    /**
     * Creates a new resource object with the given state.
     * @param dto The DTO containing the resource properties and an optional state.
     * @returns A new resource object with the given state.
     * @template TState The type of the state to include in the Resource.
     */
    public static createResource<TState>(dto: ResourceDto & TState): Resource & TState {
        const resource = new Resource(dto);

        return resource as Resource & TState;
    }

    /**
     * Creates a new FormsResource instance with the provided state.
     * @param dto The DTO to use for the FormsResource.
     * @returns A new FormsResource instance with the provided state.
     * @template TState The type of the state to include in the FormsResource. This will be void in most cases, because Forms handle their state through the _templates property.
     */
    public static createFormResource<TState>(dto: FormsResourceDto & TState): FormsResource & TState {
        const resource = new FormsResource(dto);

        return resource as FormsResource & TState;
    }

    /**
     * Creates a new ProblemDetails instance with the provided dto.
     * @param dto The DTO to use for the ProblemDetails.
     * @returns The newly created `ProblemDetails` resource.
     */
    public static createProblemDetails(dto: ProblemDetailsDto & ResourceDto): ProblemDetails {
        const resource = new ProblemDetails(dto);

        return resource;
    }

    /**
     * Creates a new ListResource instance with the given state.
     * @param dto The DTO for the ListResource.
     * @returns A new ListResource instance with the provided DTO and state.
     * @template TList The type of the list items.
     * @template TState The type of the state to include in the ListResource. Note that this is not the type of the list items, but the type of the state that is included in the ListResource itself.
     */
    public static createListResource<TList, TState = void>(dto: ListResourceDto<TList & ResourceDto> & TState): ListResource<TList> & TState {
        const resource = new ListResource(dto);

        return resource as ListResource<TList> & TState;
    }

    /**
     * Creates a new PagedListResource instance with the given state.
     * @param dto The DTO for the PagedListResource.
     * @returns A new PagedListResource instance with the provided DTO and state.
     * @template TList The type of the list items.
     * @template TState The type of the state to include in the PagedListResource. Note that this is not the type of the list items, but the type of the state that is included in the ListResource itself.
     */
    public static createPagedListResource<TList, TState = void>(dto: PagedListResourceDto<TList & ResourceDto> & TState): PagedListResource<TList> & TState {
        const resource = new PagedListResource(dto);

        return resource as PagedListResource<TList> & TState;
    }

    /**
     * Creates a new PagedListFormsResource instance with the given state.
     * @param dto The DTO for the PagedListFormsResource.
     * @returns A new PagedListFormsResource instance with the provided DTO and state.
     * @template TList The type of the list items.
     * @template TState The type of the state to include in the PagedListFormsResource. Note that this is not the type of the list items, but the type of the state that is included in the ListResource itself.
     */
    public static createPagedListFormsResource<TList, TState = void>(dto: PagedListFormsResourceDto<TList & ResourceDto> & TState): PagedListFormsResource<TList> & TState {
        const resource = new PagedListFormsResource(dto);

        return resource as PagedListFormsResource<TList> & TState;
    }
}