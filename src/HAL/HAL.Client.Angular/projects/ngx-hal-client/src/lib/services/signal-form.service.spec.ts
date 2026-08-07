import { TestBed } from '@angular/core/testing';
import { beforeEach, describe, expect, it } from 'vitest';
import { SignalFormService } from './signal-form.service';
import { PropertyDto, PropertyType, SimpleValue, Template } from '../models/formsResource';

/**
 * Builds a raw `PropertyDto` fixture with sensible defaults, so individual tests only need to
 * specify the fields that are relevant to what they are verifying.
 *
 * Note: We deliberately use the untyped `PropertyDto<SimpleValue, string, string>` shape (instead
 * of trying to infer a narrower generic per call) to keep test fixtures simple. The type of
 * `value` does not affect the behavior under test.
 */
function buildProperty(
    overrides: Partial<PropertyDto<SimpleValue, string, string>> & { name: string; value: SimpleValue }
): PropertyDto<SimpleValue, string, string> {
    return {
        type: PropertyType.Text,
        ...overrides,
    };
}

/**
 * Wraps a list of `PropertyDto` fixtures into a real `Template` instance (the type expected by
 * `createSignalFormFromTemplate`).
 */
function buildTemplate(properties: PropertyDto<SimpleValue, string, string>[], title?: string): Template {
    return new Template({ properties, title });
}

/**
 * `FieldTree` navigation on the un-modeled overload of `createSignalFormFromTemplate` resolves to
 * an index-signature based type (`Record<string, unknown>`), which is awkward to navigate with
 * strict typing in tests. We cast to `any` purely for ergonomic dynamic field access; this does
 * not affect the runtime behavior being verified.
 */
function asAny(value: unknown): any {
    return value;
}

describe('SignalFormService', () => {
    let service: SignalFormService;

    beforeEach(() => {
        TestBed.configureTestingModule({});
        service = TestBed.inject(SignalFormService);
    });
    
    describe('createSignalFormFromTemplate', () => {
        describe('baseline: a model with a single string property', () => {
            it('builds a model and a form field from a single Text property', () => {
                const template = buildTemplate([
                    buildProperty({ name: 'name', value: 'John Doe' }),
                ]);

                const signalForm = TestBed.runInInjectionContext(() => service.createSignalFormFromTemplate(template));

                expect(signalForm.model()).toEqual({ name: 'John Doe' });
                expect(asAny(signalForm.form).name().value()).toBe('John Doe');
            });
        });

        describe('all simple PropertyType values', () => {
            const simplePropertyTypeCases: Array<[PropertyType, SimpleValue]> = [
                [PropertyType.Hidden, 'hidden-value'],
                [PropertyType.Text, 'text-value'],
                [PropertyType.Textarea, 'textarea-value'],
                [PropertyType.Search, 'search-value'],
                [PropertyType.Tel, '+1-555-0100'],
                [PropertyType.Url, 'https://example.com'],
                [PropertyType.Password, 'p@ssw0rd'],
                [PropertyType.Date, new Date('2024-01-15')],
                [PropertyType.Month, '2024-01'],
                [PropertyType.Week, '2024-W03'],
                [PropertyType.Time, '13:45'],
                [PropertyType.DatetimeLocal, '2024-01-15T13:45'],
                [PropertyType.Number, 42],
                [PropertyType.Range, 5],
                [PropertyType.Color, '#ff0000'],
                [PropertyType.Bool, true],
                [PropertyType.DatetimeOffset, new Date('2024-01-15T13:45:00Z')],
                [PropertyType.Duration, 'PT1H30M'],
                [PropertyType.Image, 'https://example.com/image.png'],
                [PropertyType.File, 'file.txt'],
                [PropertyType.Percent, 0.5],
                [PropertyType.Currency, 19.99],
            ];

            it.each(simplePropertyTypeCases)(
                'passes the value through unchanged for PropertyType.%s',
                (type, value) => {

                    const template = buildTemplate([
                        buildProperty({ name: 'data', type, value }),
                    ]);

                    const signalForm = TestBed.runInInjectionContext(() => service.createSignalFormFromTemplate(template));

                    expect(signalForm.model()).toEqual({ data: value });
                    expect(asAny(signalForm.form).data().value()).toEqual(value);
                }
            );
        });

        describe('null property value on text-like PropertyTypes', () => {
            // Regression tests: real HAL-FORMS servers commonly return `value: null` for an unset
            // property on a "new" resource template. For a plain-text-like PropertyType, defaulting
            // this to `null` (instead of an empty string) causes Angular Signal Forms' native
            // `<input>` binding to apply a numeric-parse heuristic (since `null` looks like "could be
            // a number field") - which then reports a spurious 'parse' validation error (and,
            // because the model value never gets updated when parsing fails, an equally spurious
            // "field is required" error) the moment the user types non-numeric text. Defaulting to
            // `''` for these types avoids ever entering that ambiguous state.
            const textLikePropertyTypeCases: PropertyType[] = [
                PropertyType.Hidden,
                PropertyType.Text,
                PropertyType.Search,
                PropertyType.Tel,
                PropertyType.Url,
                PropertyType.Email,
                PropertyType.Password,
                PropertyType.Color,
                PropertyType.Percent,
                PropertyType.Currency,
            ];

            it.each(textLikePropertyTypeCases)(
                'defaults a null value to an empty string for PropertyType.%s',
                (type) => {

                    const template = buildTemplate([
                        buildProperty({ name: 'data', type, value: null }),
                    ]);

                    const signalForm = TestBed.runInInjectionContext(() => service.createSignalFormFromTemplate(template));

                    expect(signalForm.model()).toEqual({ data: '' });
                }
            );

            it('defaults an omitted (undefined) type to an empty string, same as PropertyType.Text', () => {

                const template = buildTemplate([
                    { name: 'data', value: null },
                ]);

                const signalForm = TestBed.runInInjectionContext(() => service.createSignalFormFromTemplate(template));

                expect(signalForm.model()).toEqual({ data: '' });
            });

            // Types rendered via a dedicated custom control (not a bare native <input>) are
            // unaffected by the native-input parse heuristic and should keep `null` as their
            // "unset" default, matching their actual runtime value type (number | Date | boolean | null).
            const nonTextLikePropertyTypeCases: PropertyType[] = [
                PropertyType.Number,
                PropertyType.Range,
                PropertyType.Bool,
                PropertyType.Date,
                PropertyType.Month,
                PropertyType.Week,
                PropertyType.Time,
                PropertyType.DatetimeLocal,
                PropertyType.DatetimeOffset,
                PropertyType.Duration,
                PropertyType.Image,
                PropertyType.File,
            ];

            it.each(nonTextLikePropertyTypeCases)(
                'still defaults a null value to null (not an empty string) for PropertyType.%s',
                (type) => {

                    const template = buildTemplate([
                        buildProperty({ name: 'data', type, value: null }),
                    ]);

                    const signalForm = TestBed.runInInjectionContext(() => service.createSignalFormFromTemplate(template));

                    expect(signalForm.model()).toEqual({ data: null });
                }
            );

            it('does not throw a parse error and accepts free-form text after a null-defaulted value is edited', () => {
                // End-to-end style check of the actual bug report: build a form the same way
                // createSignalFormFromTemplate would for a "new" resource (value: null), then
                // simulate the user typing free-form text into the field's value and assert no
                // validation error of kind 'parse' is present and the value round-trips correctly.
                const template = buildTemplate([
                    buildProperty({ name: 'name', required: true, value: null }),
                ]);

                const signalForm = TestBed.runInInjectionContext(() => service.createSignalFormFromTemplate(template));

                asAny(signalForm.form).name().value.set('My new blog');

                expect(signalForm.model()).toEqual({ name: 'My new blog' });
                expect(asAny(signalForm.form).name().errors().some((e: { kind: string }) => e.kind === 'parse')).toBe(false);
                expect(asAny(signalForm.form).name().valid()).toBe(true);
            });
        });

        describe('special validators', () => {
            it('marks a field as required when the property is required', () => {

                const template = buildTemplate([
                    buildProperty({ name: 'name', required: true, value: '' }),
                ]);

                const signalForm = TestBed.runInInjectionContext(() => service.createSignalFormFromTemplate(template));

                expect(asAny(signalForm.form).name().required()).toBe(true);
                expect(asAny(signalForm.form).name().valid()).toBe(false);
            });

            it('does not mark a field as required when the property is not required', () => {

                const template = buildTemplate([
                    buildProperty({ name: 'name', value: '' }),
                ]);

                const signalForm = TestBed.runInInjectionContext(() => service.createSignalFormFromTemplate(template));

                expect(asAny(signalForm.form).name().required()).toBe(false);
                expect(asAny(signalForm.form).name().valid()).toBe(true);
            });

            it('treats a valid email address as valid', () => {

                const template = buildTemplate([
                    buildProperty({ name: 'email', type: PropertyType.Email, value: 'foo@example.com' }),
                ]);

                const signalForm = TestBed.runInInjectionContext(() => service.createSignalFormFromTemplate(template));

                expect(asAny(signalForm.form).email().valid()).toBe(true);
            });

            it('treats an invalid email address as invalid', () => {

                const template = buildTemplate([
                    buildProperty({ name: 'email', type: PropertyType.Email, value: 'not-an-email' }),
                ]);

                const signalForm = TestBed.runInInjectionContext(() => service.createSignalFormFromTemplate(template));

                expect(asAny(signalForm.form).email().valid()).toBe(false);
            });

            it('exposes the min validator on a number property and validates it', () => {

                const validTemplate = buildTemplate([
                    buildProperty({ name: 'age', type: PropertyType.Number, min: 5, value: 10 }),
                ]);
                const invalidTemplate = buildTemplate([
                    buildProperty({ name: 'age', type: PropertyType.Number, min: 5, value: 2 }),
                ]);

                const validSignalForm = TestBed.runInInjectionContext(() => service.createSignalFormFromTemplate(validTemplate));
                const invalidSignalForm = TestBed.runInInjectionContext(() => service.createSignalFormFromTemplate(invalidTemplate));

                expect(asAny(validSignalForm.form).age().min()).toBe(5);
                expect(asAny(validSignalForm.form).age().valid()).toBe(true);
                expect(asAny(invalidSignalForm.form).age().valid()).toBe(false);
            });

            it('exposes the max validator on a number property and validates it', () => {

                const validTemplate = buildTemplate([
                    buildProperty({ name: 'age', type: PropertyType.Number, max: 10, value: 5 }),
                ]);
                const invalidTemplate = buildTemplate([
                    buildProperty({ name: 'age', type: PropertyType.Number, max: 10, value: 20 }),
                ]);

                const validSignalForm = TestBed.runInInjectionContext(() => service.createSignalFormFromTemplate(validTemplate));
                const invalidSignalForm = TestBed.runInInjectionContext(() => service.createSignalFormFromTemplate(invalidTemplate));

                expect(asAny(validSignalForm.form).age().max()).toBe(10);
                expect(asAny(validSignalForm.form).age().valid()).toBe(true);
                expect(asAny(invalidSignalForm.form).age().valid()).toBe(false);
            });

            it('exposes the minLength validator on a text property and validates it', () => {

                const validTemplate = buildTemplate([
                    buildProperty({ name: 'username', minLength: 3, value: 'abc' }),
                ]);
                const invalidTemplate = buildTemplate([
                    buildProperty({ name: 'username', minLength: 3, value: 'ab' }),
                ]);

                const validSignalForm = TestBed.runInInjectionContext(() => service.createSignalFormFromTemplate(validTemplate));
                const invalidSignalForm = TestBed.runInInjectionContext(() => service.createSignalFormFromTemplate(invalidTemplate));

                expect(asAny(validSignalForm.form).username().minLength()).toBe(3);
                expect(asAny(validSignalForm.form).username().valid()).toBe(true);
                expect(asAny(invalidSignalForm.form).username().valid()).toBe(false);
            });

            it('exposes the maxLength validator on a text property and validates it', () => {

                const validTemplate = buildTemplate([
                    buildProperty({ name: 'username', maxLength: 5, value: 'abc' }),
                ]);
                const invalidTemplate = buildTemplate([
                    buildProperty({ name: 'username', maxLength: 5, value: 'abcdef' }),
                ]);

                const validSignalForm = TestBed.runInInjectionContext(() => service.createSignalFormFromTemplate(validTemplate));
                const invalidSignalForm = TestBed.runInInjectionContext(() => service.createSignalFormFromTemplate(invalidTemplate));

                expect(asAny(validSignalForm.form).username().maxLength()).toBe(5);
                expect(asAny(validSignalForm.form).username().valid()).toBe(true);
                expect(asAny(invalidSignalForm.form).username().valid()).toBe(false);
            });

            it('exposes a regex pattern validator on a text property and validates it', () => {

                const validTemplate = buildTemplate([
                    buildProperty({ name: 'code', regex: '^[a-z]+$', value: 'abc' }),
                ]);
                const invalidTemplate = buildTemplate([
                    buildProperty({ name: 'code', regex: '^[a-z]+$', value: 'ABC' }),
                ]);

                const validSignalForm = TestBed.runInInjectionContext(() => service.createSignalFormFromTemplate(validTemplate));
                const invalidSignalForm = TestBed.runInInjectionContext(() => service.createSignalFormFromTemplate(invalidTemplate));

                const patterns: RegExp[] = asAny(validSignalForm.form).code().pattern();
                expect(patterns.some(p => p.source === '^[a-z]+$')).toBe(true);
                expect(asAny(validSignalForm.form).code().valid()).toBe(true);
                expect(asAny(invalidSignalForm.form).code().valid()).toBe(false);
            });

            // Regression tests: HAL-FORMS JSON returned by a real server typically includes
            // max/min/maxLength/minLength explicitly set to `null` (rather than omitting them)
            // when they are unset. `createSignalFormFromTemplate` must tolerate this and not apply
            // (or crash while trying to apply) a validator for a `null` constraint. Previously, a
            // `!== undefined` check let `null` values through into `max`/`min`/`maxLength`/
            // `minLength`, which then crashed with e.g. "maxLength is not a function".
            it('does not throw and does not apply a max validator when max is null', () => {

                const template = buildTemplate([
                    buildProperty({ name: 'age', type: PropertyType.Number, max: null as unknown as number, value: 5 }),
                ]);

                expect(() => TestBed.runInInjectionContext(() => service.createSignalFormFromTemplate(template))).not.toThrow();

                const signalForm = TestBed.runInInjectionContext(() => service.createSignalFormFromTemplate(template));
                expect(asAny(signalForm.form).age().valid()).toBe(true);
            });

            it('does not throw and does not apply a min validator when min is null', () => {

                const template = buildTemplate([
                    buildProperty({ name: 'age', type: PropertyType.Number, min: null as unknown as number, value: 5 }),
                ]);

                expect(() => TestBed.runInInjectionContext(() => service.createSignalFormFromTemplate(template))).not.toThrow();

                const signalForm = TestBed.runInInjectionContext(() => service.createSignalFormFromTemplate(template));
                expect(asAny(signalForm.form).age().valid()).toBe(true);
            });

            it('does not throw and does not apply a maxLength validator when maxLength is null', () => {

                const template = buildTemplate([
                    buildProperty({ name: 'name', maxLength: null as unknown as number, value: 'abc' }),
                ]);

                expect(() => TestBed.runInInjectionContext(() => service.createSignalFormFromTemplate(template))).not.toThrow();

                const signalForm = TestBed.runInInjectionContext(() => service.createSignalFormFromTemplate(template));
                expect(asAny(signalForm.form).name().valid()).toBe(true);
            });

            it('does not throw and does not apply a minLength validator when minLength is null', () => {

                const template = buildTemplate([
                    buildProperty({ name: 'name', minLength: null as unknown as number, value: 'abc' }),
                ]);

                expect(() => TestBed.runInInjectionContext(() => service.createSignalFormFromTemplate(template))).not.toThrow();

                const signalForm = TestBed.runInInjectionContext(() => service.createSignalFormFromTemplate(template));
                expect(asAny(signalForm.form).name().valid()).toBe(true);
            });

            it('still applies max/min/maxLength/minLength validators when they are 0 (falsy but not null)', () => {

                const template = buildTemplate([
                    buildProperty({ name: 'name', maxLength: 0, minLength: 0, value: '' }),
                ]);

                const signalForm = TestBed.runInInjectionContext(() => service.createSignalFormFromTemplate(template));

                expect(asAny(signalForm.form).name().maxLength()).toBe(0);
                expect(asAny(signalForm.form).name().minLength()).toBe(0);
            });
        });

        describe('Object property type', () => {
            it('builds a nested model from the default template of an Object property', () => {

                const addressProperty = buildProperty({
                    name: 'address',
                    type: PropertyType.Object,
                    value: null,
                    _templates: {
                        default: {
                            properties: [
                                buildProperty({ name: 'street', required: true, value: 'Main St' }),
                                buildProperty({ name: 'city', value: 'Springfield' }),
                            ],
                        },
                    },
                });
                const template = buildTemplate([addressProperty]);

                const signalForm = TestBed.runInInjectionContext(() => service.createSignalFormFromTemplate(template));

                expect(signalForm.model()).toEqual({
                    address: { street: 'Main St', city: 'Springfield' },
                });
            });

            it('propagates validation from the default template into the nested fields', () => {

                const addressProperty = buildProperty({
                    name: 'address',
                    type: PropertyType.Object,
                    value: null,
                    _templates: {
                        default: {
                            properties: [
                                buildProperty({ name: 'street', required: true, value: '' }),
                                buildProperty({ name: 'city', value: 'Springfield' }),
                            ],
                        },
                    },
                });
                const template = buildTemplate([addressProperty]);

                const signalForm = TestBed.runInInjectionContext(() => service.createSignalFormFromTemplate(template));

                expect(asAny(signalForm.form).address.street().required()).toBe(true);
                expect(asAny(signalForm.form).address.street().valid()).toBe(false);
                expect(asAny(signalForm.form).address.city().required()).toBe(false);
            });

            it('throws when an Object property has no default template', () => {

                const addressProperty = buildProperty({
                    name: 'address',
                    type: PropertyType.Object,
                    value: null,
                });
                const template = buildTemplate([addressProperty]);

                expect(() => service.createSignalFormFromTemplate(template)).toThrow();
            });
        });

        describe('Collection property type', () => {
            it('builds an array model from the indexed templates, using each item template own values', () => {

                const itemsProperty = buildProperty({
                    name: 'items',
                    type: PropertyType.Collection,
                    value: null,
                    _templates: {
                        default: { properties: [buildProperty({ name: 'name', required: true, value: '' })] },
                        '0': { properties: [buildProperty({ name: 'name', value: 'Item A' })] },
                        '1': { properties: [buildProperty({ name: 'name', value: 'Item B' })] },
                    },
                });
                const template = buildTemplate([itemsProperty]);

                const signalForm = TestBed.runInInjectionContext(() => service.createSignalFormFromTemplate(template));

                expect(signalForm.model()).toEqual({
                    items: [{ name: 'Item A' }, { name: 'Item B' }],
                });
            });

            it('applies the default template validators to every item via applyEach', () => {

                const itemsProperty = buildProperty({
                    name: 'items',
                    type: PropertyType.Collection,
                    value: null,
                    _templates: {
                        default: { properties: [buildProperty({ name: 'name', required: true, value: '' })] },
                        '0': { properties: [buildProperty({ name: 'name', value: '' })] },
                        '1': { properties: [buildProperty({ name: 'name', value: 'Item B' })] },
                    },
                });
                const template = buildTemplate([itemsProperty]);

                const signalForm = TestBed.runInInjectionContext(() => service.createSignalFormFromTemplate(template));

                expect(asAny(signalForm.form).items[0].name().required()).toBe(true);
                expect(asAny(signalForm.form).items[0].name().valid()).toBe(false);
                expect(asAny(signalForm.form).items[1].name().valid()).toBe(true);
            });

            it('skips default validators for items whose indexed template uses different property names', () => {

                const itemsProperty = buildProperty({
                    name: 'items',
                    type: PropertyType.Collection,
                    value: null,
                    _templates: {
                        default: { properties: [buildProperty({ name: 'name', required: true, value: '' })] },
                        '0': { properties: [buildProperty({ name: 'title', value: 'Item A' })] },
                    },
                });
                const template = buildTemplate([itemsProperty]);

                const signalForm = TestBed.runInInjectionContext(() => service.createSignalFormFromTemplate(template));

                expect(signalForm.model()).toEqual({ items: [{ title: 'Item A' }] });
                // The 'required' rule from the default template targets a 'name' field, which does not
                // exist on this item (it only has 'title'), so it is silently skipped.
                expect(asAny(signalForm.form).items[0].title().required()).toBe(false);
                expect(asAny(signalForm.form).items[0].title().valid()).toBe(true);
            });

            it('produces an empty array when a Collection property has no templates', () => {

                const itemsProperty = buildProperty({
                    name: 'items',
                    type: PropertyType.Collection,
                    value: null,
                });
                const template = buildTemplate([itemsProperty]);

                const signalForm = TestBed.runInInjectionContext(() => service.createSignalFormFromTemplate(template));

                expect(signalForm.model()).toEqual({ items: [] });
            });
        });

        describe('model override overload', () => {
            it('uses the provided model values instead of the template-derived values', () => {

                const template = buildTemplate([
                    buildProperty({ name: 'name', required: true, value: 'Template value' }),
                ]);

                const signalForm = TestBed.runInInjectionContext(() => service.createSignalFormFromTemplate(template, { name: 'Override value' }));

                expect(signalForm.model()).toEqual({ name: 'Override value' });
            });

            it('still applies template-derived validation when a model override is provided', () => {

                const template = buildTemplate([
                    buildProperty({ name: 'name', required: true, value: 'Template value' }),
                ]);

                const signalForm = TestBed.runInInjectionContext(() => service.createSignalFormFromTemplate(template, { name: '' }));

                expect(asAny(signalForm.form).name().required()).toBe(true);
                expect(asAny(signalForm.form).name().valid()).toBe(false);
            });
        });

        describe('options (inline)', () => {
            it('resolves a single selected value from options.selectedValues', () => {

                const template = buildTemplate([
                    buildProperty({
                        name: 'color',
                        value: null,
                        options: {
                            inline: [
                                { prompt: 'Red', value: 'red' },
                                { prompt: 'Blue', value: 'blue' },
                            ],
                            selectedValues: ['blue'],
                        },
                    }),
                ]);

                const signalForm = TestBed.runInInjectionContext(() => service.createSignalFormFromTemplate(template));

                expect(signalForm.model()).toEqual({ color: 'blue' });
            });

            it('resolves all selected values as an array when maxItems allows multiple selections', () => {

                const template = buildTemplate([
                    buildProperty({
                        name: 'colors',
                        value: null,
                        options: {
                            inline: [
                                { prompt: 'Red', value: 'red' },
                                { prompt: 'Blue', value: 'blue' },
                            ],
                            selectedValues: ['red', 'blue'],
                            maxItems: 2,
                        },
                    }),
                ]);

                const signalForm = TestBed.runInInjectionContext(() => service.createSignalFormFromTemplate(template));

                expect(signalForm.model()).toEqual({ colors: ['red', 'blue'] });
            });

            it('falls back to the property value when selectedValues is empty', () => {

                const template = buildTemplate([
                    buildProperty({
                        name: 'color',
                        value: 'green',
                        options: {
                            inline: [
                                { prompt: 'Red', value: 'red' },
                                { prompt: 'Blue', value: 'blue' },
                            ],
                            selectedValues: [],
                        },
                    }),
                ]);

                const signalForm = TestBed.runInInjectionContext(() => service.createSignalFormFromTemplate(template));

                expect(signalForm.model()).toEqual({ color: 'green' });
            });

            it('falls back to null when there is neither a value nor selectedValues', () => {

                const template = buildTemplate([
                    buildProperty({
                        name: 'color',
                        value: null,
                        options: {
                            inline: [
                                { prompt: 'Red', value: 'red' },
                                { prompt: 'Blue', value: 'blue' },
                            ],
                            selectedValues: [],
                        },
                    }),
                ]);

                const signalForm = TestBed.runInInjectionContext(() => service.createSignalFormFromTemplate(template));

                expect(signalForm.model()).toEqual({ color: null });
            });
        });
    });

    describe('buildModelFromTemplate', () => {
        it('builds a default model with each property\'s own value for simple properties', () => {
            const template = buildTemplate([
                buildProperty({ name: 'name', value: 'John Doe' }),
                buildProperty({ name: 'age', type: PropertyType.Number, value: 42 }),
            ]);

            const model = service.buildModelFromTemplate(template);

            expect(model).toEqual({ name: 'John Doe', age: 42 });
        });

        it('recursively builds a nested model for an Object property using its default template', () => {
            const addressProperty = buildProperty({
                name: 'address',
                type: PropertyType.Object,
                value: null,
                _templates: {
                    default: {
                        properties: [
                            buildProperty({ name: 'street', value: 'Main St' }),
                            buildProperty({ name: 'city', value: 'Springfield' }),
                        ],
                    },
                },
            });
            const template = buildTemplate([addressProperty]);

            const model = service.buildModelFromTemplate(template);

            expect(model).toEqual({
                address: { street: 'Main St', city: 'Springfield' },
            });
        });

        it('builds an array model for a Collection property from its indexed templates', () => {
            const itemsProperty = buildProperty({
                name: 'items',
                type: PropertyType.Collection,
                value: null,
                _templates: {
                    default: { properties: [buildProperty({ name: 'name', value: '' })] },
                    '0': { properties: [buildProperty({ name: 'name', value: 'Item A' })] },
                    '1': { properties: [buildProperty({ name: 'name', value: 'Item B' })] },
                },
            });
            const template = buildTemplate([itemsProperty]);

            const model = service.buildModelFromTemplate(template);

            expect(model).toEqual({ items: [{ name: 'Item A' }, { name: 'Item B' }] });
        });

        it('resolves a value from options.selectedValues, same as the model built internally by createSignalFormFromTemplate', () => {
            const template = buildTemplate([
                buildProperty({
                    name: 'color',
                    value: null,
                    options: {
                        inline: [
                            { prompt: 'Red', value: 'red' },
                            { prompt: 'Blue', value: 'blue' },
                        ],
                        selectedValues: ['blue'],
                    },
                }),
            ]);

            const model = service.buildModelFromTemplate(template);

            expect(model).toEqual({ color: 'blue' });
        });

        it('is useful for building a default item value for a new Collection entry, given only the item template', () => {
            // This mirrors the primary intended use case: a consumer (e.g. a "collection input" component)
            // that needs to build a correctly-shaped default value for a brand new item, given only the
            // collection property's own default item template - without needing a full SignalForm/FieldTree.
            const itemsProperty = buildProperty({
                name: 'items',
                type: PropertyType.Collection,
                value: null,
                _templates: {
                    default: {
                        properties: [
                            buildProperty({ name: 'name', value: '' }),
                            buildProperty({ name: 'quantity', type: PropertyType.Number, value: 1 }),
                        ],
                    },
                },
            });

            const itemTemplate = itemsProperty._templates!['default']!;
            const newItem = service.buildModelFromTemplate(itemTemplate as Template);

            expect(newItem).toEqual({ name: '', quantity: 1 });
        });

        it('throws when an Object property has no default template', () => {
            const addressProperty = buildProperty({
                name: 'address',
                type: PropertyType.Object,
                value: null,
            });
            const template = buildTemplate([addressProperty]);

            expect(() => service.buildModelFromTemplate(template)).toThrow();
        });

        it('produces an empty array when a Collection property has no templates', () => {
            const itemsProperty = buildProperty({
                name: 'items',
                type: PropertyType.Collection,
                value: null,
            });
            const template = buildTemplate([itemsProperty]);

            const model = service.buildModelFromTemplate(template);

            expect(model).toEqual({ items: [] });
        });
    });
});
