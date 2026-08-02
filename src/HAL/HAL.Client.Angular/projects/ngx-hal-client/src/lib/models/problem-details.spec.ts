import { describe, expect, it } from 'vitest';
import { ProblemDetails, ProblemDetailsDto } from './problem-details';
import { Resource, ResourceDto } from './resource';

describe('ProblemDetails', () => {
  function buildDto(overrides: Partial<ProblemDetailsDto> = {}): ProblemDetailsDto & ResourceDto {
    return {
      _links: { self: [{ href: '/errors/1' }] },
      type: 'https://example.com/errors/not-found',
      title: 'Not Found',
      status: 404,
      detail: 'The item was not found.',
      instance: '/items/1',
      ...overrides,
    };
  }

  it('exposes the RFC 7807 fields as instance properties', () => {
    const problemDetails = new ProblemDetails(buildDto());

    expect(problemDetails.type).toBe('https://example.com/errors/not-found');
    expect(problemDetails.title).toBe('Not Found');
    expect(problemDetails.status).toBe(404);
    expect(problemDetails.detail).toBe('The item was not found.');
    expect(problemDetails.instance).toBe('/items/1');
  });

  it('is a Resource', () => {
    const problemDetails = new ProblemDetails(buildDto());

    expect(problemDetails).toBeInstanceOf(Resource);
  });

  describe('isProblemDetails', () => {
    it('returns true for a ProblemDetails instance', () => {
      expect(ProblemDetails.isProblemDetails(new ProblemDetails(buildDto()))).toBe(true);
    });

    it('returns false for a plain Resource', () => {
      const resource = new Resource({ _links: { self: [{ href: '/items/1' }] } });

      expect(ProblemDetails.isProblemDetails(resource)).toBe(false);
    });

    it('returns false for a non-resource value', () => {
      expect(ProblemDetails.isProblemDetails({ status: 404 })).toBe(false);
    });
  });

  describe('containsProblemDetailsInformation', () => {
    it('returns true for a ProblemDetails instance', () => {
      expect(ProblemDetails.containsProblemDetailsInformation(new ProblemDetails(buildDto()))).toBe(true);
    });

    it('returns true for a plain Resource that has a valid http status', () => {
      const resource = new Resource({
        _links: { self: [{ href: '/items/1' }] },
      }) as Resource & { status: number };
      resource.status = 500;

      expect(ProblemDetails.containsProblemDetailsInformation(resource)).toBe(true);
    });

    it('returns false for a plain Resource without a status', () => {
      const resource = new Resource({ _links: { self: [{ href: '/items/1' }] } });

      expect(ProblemDetails.containsProblemDetailsInformation(resource)).toBeFalsy();
    });

    it('returns false for null', () => {
      expect(ProblemDetails.containsProblemDetailsInformation(null)).toBeFalsy();
    });

    it('returns false for a non-resource object', () => {
      expect(ProblemDetails.containsProblemDetailsInformation({ status: 404 })).toBeFalsy();
    });
  });

  describe('isProblemDetailsDto', () => {
    it('returns true for an object with a valid numeric status', () => {
      expect(ProblemDetails.isProblemDetailsDto({ status: 404 })).toBe(true);
    });

    it('returns false when status is missing', () => {
      expect(ProblemDetails.isProblemDetailsDto({ title: 'Not Found' })).toBe(false);
    });

    it('returns false when status is not a number', () => {
      expect(ProblemDetails.isProblemDetailsDto({ status: '404' })).toBe(false);
    });

    it('returns false for null', () => {
      expect(ProblemDetails.isProblemDetailsDto(null)).toBe(false);
    });

    it('returns false for a non-object value', () => {
      expect(ProblemDetails.isProblemDetailsDto('not-an-object')).toBe(false);
    });
  });

  describe('hasValidHttpStatus', () => {
    it('returns true for a status within the valid HTTP range', () => {
      expect(ProblemDetails.hasValidHttpStatus({ status: 200 })).toBe(true);
      expect(ProblemDetails.hasValidHttpStatus({ status: 599 })).toBe(true);
    });

    it('returns false for a status below 100', () => {
      expect(ProblemDetails.hasValidHttpStatus({ status: 99 })).toBe(false);
    });

    it('returns false for a status of 600 or above', () => {
      expect(ProblemDetails.hasValidHttpStatus({ status: 600 })).toBe(false);
    });

    it('returns false for a non-integer status', () => {
      expect(ProblemDetails.hasValidHttpStatus({ status: 200.5 })).toBe(false);
    });

    it('returns false when the status property is missing', () => {
      expect(ProblemDetails.hasValidHttpStatus({})).toBe(false);
    });
  });
});
