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
