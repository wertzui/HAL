/**
 * A page is commonly used to represent a page of a paged collection.
 * It is mostly used as the TState of a @see PagedListResource.
 */
export interface Page {
  currentPage?: number;
  totalPages?: number;
}