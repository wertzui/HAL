import { describe, expect, it } from 'vitest';
import { PagedListFormsResource, PagedListFormsResourceDto } from './pagedListFormsResource';
import { Template } from './formsResource';

interface ItemDto {
  name: string;
}

describe('PagedListFormsResource', () => {
  function buildDto(overrides: Partial<PagedListFormsResourceDto<ItemDto>> = {}): PagedListFormsResourceDto<ItemDto> {
    return {
      _links: { self: [{ href: '/items?page=1' }] },
      currentPage: 1,
      totalPages: 1,
      ...overrides,
    };
  }

  it('exposes paging state alongside the templates', () => {
    const resource = new PagedListFormsResource(buildDto({
      currentPage: 2,
      totalPages: 4,
      _templates: { search: { properties: [] } },
    }));

    expect(resource.currentPage).toBe(2);
    expect(resource.totalPages).toBe(4);
    expect(resource._templates['search']).toBeInstanceOf(Template);
  });

  it('defaults _templates to an empty object when missing', () => {
    const resource = new PagedListFormsResource(buildDto());

    expect(resource._templates).toEqual({});
  });

  describe('getTemplate', () => {
    it('returns the template with the given name', () => {
      const resource = new PagedListFormsResource(buildDto({
        _templates: { search: { properties: [] } },
      }));

      expect(resource.getTemplate('search')).toBeInstanceOf(Template);
    });

    it('throws when no template with the given name exists', () => {
      const resource = new PagedListFormsResource(buildDto({
        _templates: { search: { properties: [] } },
      }));

      expect(() => resource.getTemplate('missing')).toThrow(/does not have a _template with the name/);
    });
  });

  it('still exposes embedded items', () => {
    const resource = new PagedListFormsResource(buildDto({
      _embedded: { items: [{ _links: { self: [{ href: '/items/1' }] }, name: 'Item 1' }] as (ItemDto & { _links: { self: { href: string }[] } })[] },
    }));

    expect(resource._embedded.items).toHaveLength(1);
    expect(resource._embedded.items[0].name).toBe('Item 1');
  });
});
