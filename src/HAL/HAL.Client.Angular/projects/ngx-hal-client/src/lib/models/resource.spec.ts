import { describe, expect, it } from 'vitest';
import { Resource, ResourceDto } from './resource';

describe('Resource', () => {
  describe('constructor', () => {
    it('throws when the self link is missing', () => {
      const dto = { _links: {} } as unknown as ResourceDto;

      expect(() => new Resource(dto)).toThrow(/self link is missing/);
    });

    it('throws when _links is missing entirely', () => {
      const dto = {} as unknown as ResourceDto;

      expect(() => new Resource(dto)).toThrow(/self link is missing/);
    });

    it('creates Link instances for every link relation', () => {
      const dto: ResourceDto = {
        _links: {
          self: [{ href: '/items/1' }],
          up: [{ href: '/items' }],
        },
      };

      const resource = new Resource(dto);

      expect(resource._links['self'][0].href).toBe('/items/1');
      expect(resource._links['up']![0].href).toBe('/items');
    });

    it('defaults _embedded to an empty object when missing', () => {
      const dto: ResourceDto = { _links: { self: [{ href: '/items/1' }] } };

      const resource = new Resource(dto);

      expect(resource._embedded).toEqual({});
    });

    it('creates Resource instances for every embedded resource', () => {
      const dto: ResourceDto = {
        _links: { self: [{ href: '/items/1' }] },
        _embedded: {
          children: [
            { _links: { self: [{ href: '/items/1/children/1' }] } },
          ],
        },
      };

      const resource = new Resource(dto);

      expect(resource._embedded['children']).toHaveLength(1);
      expect(resource._embedded['children'][0]).toBeInstanceOf(Resource);
      expect(resource._embedded['children'][0]._links['self'][0].href).toBe('/items/1/children/1');
    });

    it('throws when an embedded resource is missing a self link', () => {
      const dto: ResourceDto = {
        _links: { self: [{ href: '/items/1' }] },
        _embedded: {
          children: [{ _links: {} } as unknown as ResourceDto],
        },
      };

      expect(() => new Resource(dto)).toThrow(/self link is missing/);
    });

    it('filters out falsy embedded resource dtos', () => {
      const dto: ResourceDto = {
        _links: { self: [{ href: '/items/1' }] },
        _embedded: {
          children: [null as unknown as ResourceDto],
        },
      };

      const resource = new Resource(dto);

      expect(resource._embedded['children']).toEqual([]);
    });

    it('copies over plain (non-reserved) properties from the dto', () => {
      const dto = {
        _links: { self: [{ href: '/items/1' }] },
        name: 'Item 1',
        count: 5,
      } as ResourceDto & { name: string; count: number };

      const resource = new Resource(dto) as Resource & { name: string; count: number };

      expect(resource.name).toBe('Item 1');
      expect(resource.count).toBe(5);
    });

    describe('date parsing', () => {
      it('parses an ISO 8601 date-time string property into a Date', () => {
        const dto = {
          _links: { self: [{ href: '/items/1' }] },
          createdAt: '2024-01-15T13:45:00Z',
        } as ResourceDto & { createdAt: string };

        const resource = new Resource(dto) as Resource & { createdAt: unknown };

        expect(resource.createdAt).toBeInstanceOf(Date);
        expect((resource.createdAt as Date).toISOString()).toBe('2024-01-15T13:45:00.000Z');
      });

      it('parses an ISO 8601 date-only string property into a Date', () => {
        const dto = {
          _links: { self: [{ href: '/items/1' }] },
          birthDate: '2024-01-15',
        } as ResourceDto & { birthDate: string };

        const resource = new Resource(dto) as Resource & { birthDate: unknown };

        expect(resource.birthDate).toBeInstanceOf(Date);
      });

      it('parses a bare time string property into a Date', () => {
        const dto = {
          _links: { self: [{ href: '/items/1' }] },
          openingTime: '13:45',
        } as ResourceDto & { openingTime: string };

        const resource = new Resource(dto) as Resource & { openingTime: unknown };

        expect(resource.openingTime).toBeInstanceOf(Date);
      });

      it('does not convert a plain non-date string', () => {
        const dto = {
          _links: { self: [{ href: '/items/1' }] },
          name: 'Item 1',
        } as ResourceDto & { name: string };

        const resource = new Resource(dto) as Resource & { name: unknown };

        expect(resource.name).toBe('Item 1');
      });

      it('recursively parses dates inside nested objects', () => {
        const dto = {
          _links: { self: [{ href: '/items/1' }] },
          metadata: { createdAt: '2024-01-15T13:45:00Z' },
        } as ResourceDto & { metadata: { createdAt: string } };

        const resource = new Resource(dto) as Resource & { metadata: { createdAt: unknown } };

        expect(resource.metadata.createdAt).toBeInstanceOf(Date);
      });

      it('recursively parses dates inside arrays', () => {
        const dto = {
          _links: { self: [{ href: '/items/1' }] },
          timestamps: ['2024-01-15T13:45:00Z', '2024-02-20T08:00:00Z'],
        } as ResourceDto & { timestamps: string[] };

        const resource = new Resource(dto) as Resource & { timestamps: unknown[] };

        expect(resource.timestamps[0]).toBeInstanceOf(Date);
        expect(resource.timestamps[1]).toBeInstanceOf(Date);
      });

      it('leaves null and undefined properties untouched', () => {
        const dto = {
          _links: { self: [{ href: '/items/1' }] },
          note: null,
        } as ResourceDto & { note: null };

        const resource = new Resource(dto) as Resource & { note: unknown };

        expect(resource.note).toBeNull();
      });
    });
  });

  // The following tests encode two correctness/architecture findings from the code review
  // of Resource.fromDto/fromDtos/parseDates. They describe the CORRECT expected behavior and
  // currently FAIL against the existing implementation:
  //
  // Finding 1: Resource.fromDto does redundant, doubled work. It computes links/embedded/dates
  // for a dto and then constructs a "throwaway" Resource (or subclass) from the same dto, whose
  // constructor independently recomputes all of that again, before being overwritten by
  // Object.assign. This means every embedded resource dto is visited multiple times instead of
  // once, with the redundancy compounding at every level of nesting.
  //
  // Finding 2: Resource.parseDates mutates the dto that is passed in, instead of leaving the
  // caller's original object untouched.
  describe('known correctness issues (from code review)', () => {
    it('does not mutate the original dto passed to the constructor (finding 2)', () => {
      const dto = {
        _links: { self: [{ href: '/items/1' }] },
        createdAt: '2024-01-15T13:45:00Z',
      } as ResourceDto & { createdAt: string };

      new Resource(dto);

      // Resource.parseDates currently mutates the dto in place, converting this string to a
      // Date on the caller's own object, so this assertion fails today.
      expect(dto.createdAt).toBe('2024-01-15T13:45:00Z');
    });

    it('visits each embedded resource dto exactly once while constructing the tree (finding 1)', () => {
      let visitCount = 0;
      let probeValue = 'value';
      const childDto = {
        _links: { self: [{ href: '/items/1/children/1' }] },
        // A getter/setter lets us count how many times this dto's own properties are read
        // while resources are being built from it, without needing to touch any private
        // implementation details. A correct implementation should only need to look at any
        // given dto once.
        get probe(): string {
          visitCount++;
          return probeValue;
        },
        set probe(value: string) {
          probeValue = value;
        },
      };
      const dto: ResourceDto = {
        _links: { self: [{ href: '/items/1' }] },
        _embedded: { children: [childDto as unknown as ResourceDto] },
      };

      new Resource(dto);

      // Resource.fromDto builds a throwaway Resource (which independently recomputes
      // links/embedded/dates for childDto) before overwriting it with a second, correctly
      // computed set of values, and the outer constructor parses dates over the whole tree
      // again on top of that. This causes childDto to be visited multiple times instead of
      // once, so this assertion currently fails (the actual count is 5, not 1).
      expect(visitCount).toBe(1);
    });
  });

  describe('findLinks', () => {
    it('returns all links for a given relation', () => {
      const dto: ResourceDto = {
        _links: {
          self: [{ href: '/items/1' }],
          related: [{ href: '/items/2' }, { href: '/items/3' }],
        },
      };
      const resource = new Resource(dto);

      const links = resource.findLinks('related');

      expect(links).toHaveLength(2);
      expect(links.map(l => l.href)).toEqual(['/items/2', '/items/3']);
    });

    it('returns an empty array when the relation does not exist', () => {
      const dto: ResourceDto = { _links: { self: [{ href: '/items/1' }] } };
      const resource = new Resource(dto);

      expect(resource.findLinks('missing')).toEqual([]);
    });
  });

  describe('findLink', () => {
    it('returns the first link for a relation when no name is given', () => {
      const dto: ResourceDto = {
        _links: {
          self: [{ href: '/items/1' }],
          related: [{ href: '/items/2' }, { href: '/items/3' }],
        },
      };
      const resource = new Resource(dto);

      const link = resource.findLink('related');

      expect(link?.href).toBe('/items/2');
    });

    it('returns the link matching the given name', () => {
      const dto: ResourceDto = {
        _links: {
          self: [{ href: '/items/1' }],
          related: [
            { href: '/items/2', name: 'second' },
            { href: '/items/3', name: 'third' },
          ],
        },
      };
      const resource = new Resource(dto);

      const link = resource.findLink('related', 'third');

      expect(link?.href).toBe('/items/3');
    });

    it('returns undefined when no link matches the given name', () => {
      const dto: ResourceDto = {
        _links: {
          self: [{ href: '/items/1' }],
          related: [{ href: '/items/2', name: 'second' }],
        },
      };
      const resource = new Resource(dto);

      expect(resource.findLink('related', 'unknown')).toBeUndefined();
    });

    it('returns undefined when the relation does not exist', () => {
      const dto: ResourceDto = { _links: { self: [{ href: '/items/1' }] } };
      const resource = new Resource(dto);

      expect(resource.findLink('missing')).toBeUndefined();
    });
  });

  describe('findEmbedded', () => {
    it('returns embedded resources for a given relation', () => {
      const dto: ResourceDto = {
        _links: { self: [{ href: '/items/1' }] },
        _embedded: {
          children: [{ _links: { self: [{ href: '/items/1/children/1' }] } }],
        },
      };
      const resource = new Resource(dto);

      const embedded = resource.findEmbedded('children');

      expect(embedded).toHaveLength(1);
      expect(embedded[0]).toBeInstanceOf(Resource);
    });

    it('returns an empty array when the relation does not exist', () => {
      const dto: ResourceDto = { _links: { self: [{ href: '/items/1' }] } };
      const resource = new Resource(dto);

      expect(resource.findEmbedded('missing')).toEqual([]);
    });
  });

  describe('getFormLinkHrefs', () => {
    it('returns keys of _links that are valid URLs', () => {
      const dto: ResourceDto = {
        _links: {
          self: [{ href: '/items/1' }],
          'https://example.com/rels/edit-form': [{ href: '/items/1/edit' }],
          related: [{ href: '/items/2' }],
        },
      };
      const resource = new Resource(dto);

      const hrefs = resource.getFormLinkHrefs();

      expect(hrefs).toEqual(['https://example.com/rels/edit-form']);
    });

    it('returns an empty array when no relation is a URL', () => {
      const dto: ResourceDto = {
        _links: {
          self: [{ href: '/items/1' }],
          related: [{ href: '/items/2' }],
        },
      };
      const resource = new Resource(dto);

      expect(resource.getFormLinkHrefs()).toEqual([]);
    });
  });
});
