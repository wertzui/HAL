import { describe, expect, it } from 'vitest';
import { Link, LinkDto } from './link';

describe('Link', () => {
  describe('fillTemplate', () => {
    it('replaces a single placeholder in the href', () => {
      const link = Object.assign(new Link(), { href: '/items/{id}' } satisfies LinkDto);

      const result = link.fillTemplate({ id: '42' });

      expect(result).toBe('/items/42');
    });

    it('replaces multiple placeholders in the href', () => {
      const link = Object.assign(new Link(), { href: '/items/{id}/children/{childId}' } satisfies LinkDto);

      const result = link.fillTemplate({ id: '42', childId: '7' });

      expect(result).toBe('/items/42/children/7');
    });

    it('leaves the href unchanged when it has no placeholders', () => {
      const link = Object.assign(new Link(), { href: '/items' } satisfies LinkDto);

      const result = link.fillTemplate({});

      expect(result).toBe('/items');
    });
  });

  describe('fromDto', () => {
    it('creates a Link instance with all the given properties', () => {
      const dto: LinkDto = {
        href: '/items/1',
        deprecation: 'https://example.com/deprecation',
        hreflang: 'en',
        name: 'primary',
        profile: 'https://example.com/profile',
        templated: false,
        title: 'Item 1',
        type: 'application/hal+json',
      };

      const link = Link.fromDto(dto);

      expect(link).toBeInstanceOf(Link);
      expect(link).toMatchObject(dto);
    });

    it('creates a Link instance even when given null', () => {
      const link = Link.fromDto(null);

      expect(link).toBeInstanceOf(Link);
      expect(link.href).toBeUndefined();
    });

    it('creates a Link instance even when given undefined', () => {
      const link = Link.fromDto(undefined);

      expect(link).toBeInstanceOf(Link);
      expect(link.href).toBeUndefined();
    });
  });

  describe('fromDtos', () => {
    it('creates Link instances for every given dto', () => {
      const dtos: LinkDto[] = [
        { href: '/items/1' },
        { href: '/items/2' },
      ];

      const links = Link.fromDtos(dtos);

      expect(links).toHaveLength(2);
      expect(links[0]).toBeInstanceOf(Link);
      expect(links[0].href).toBe('/items/1');
      expect(links[1].href).toBe('/items/2');
    });

    it('filters out dtos without an href', () => {
      const dtos: LinkDto[] = [
        { href: '/items/1' },
        { href: '' } as LinkDto,
      ];

      const links = Link.fromDtos(dtos);

      expect(links).toHaveLength(1);
      expect(links[0].href).toBe('/items/1');
    });

    it('returns an empty array when given null', () => {
      const links = Link.fromDtos(null);

      expect(links).toEqual([]);
    });

    it('returns an empty array when given undefined', () => {
      const links = Link.fromDtos(undefined);

      expect(links).toEqual([]);
    });

    it('returns an empty array when given an empty array', () => {
      const links = Link.fromDtos([]);

      expect(links).toEqual([]);
    });
  });
});
