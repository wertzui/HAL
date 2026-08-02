import { TestBed } from '@angular/core/testing';
import { beforeEach, describe, expect, it } from 'vitest';
import { FormArray, FormControl, FormGroup, Validators } from '@angular/forms';
import { FormService } from './form.service';
import { NumberTemplate, NumberTemplates, PropertyDto, PropertyType, SimpleValue, Template, Templates } from '../models/formsResource';

function buildProperty(
    overrides: Partial<PropertyDto<SimpleValue, string, string>> & { name: string; value: SimpleValue }
): PropertyDto<SimpleValue, string, string> {
    return {
        type: PropertyType.Text,
        ...overrides,
    };
}

function buildTemplate(properties: PropertyDto<SimpleValue, string, string>[], title?: string): Template {
    return new Template({ properties, title });
}

describe('FormService', () => {
    let service: FormService;

    beforeEach(() => {
        TestBed.configureTestingModule({});
        service = TestBed.inject(FormService);
    });

    describe('createFormControl / simple properties', () => {
        it('creates a FormControl with the property value', () => {
            const property = new Template({ properties: [buildProperty({ name: 'name', value: 'John Doe' })] }).properties[0];

            const control = service.createFormControl(property);

            expect(control).toBeInstanceOf(FormControl);
            expect(control.value).toBe('John Doe');
        });

        it('resolves the value from options.selectedValues when present', () => {
            const property = new Template({
                properties: [buildProperty({
                    name: 'color',
                    value: null,
                    options: { inline: [{ prompt: 'Red', value: 'red' }], selectedValues: ['red'] },
                })],
            }).properties[0];

            const control = service.createFormControl(property);

            expect(control.value).toBe('red');
        });

        it('resolves multiple selectedValues into an array when maxItems allows it', () => {
            const property = new Template({
                properties: [buildProperty({
                    name: 'colors',
                    value: null,
                    options: {
                        inline: [{ prompt: 'Red', value: 'red' }, { prompt: 'Blue', value: 'blue' }],
                        selectedValues: ['red', 'blue'],
                        maxItems: 2,
                    },
                })],
            }).properties[0];

            const control = service.createFormControl(property);

            expect(control.value).toEqual(['red', 'blue']);
        });

        it('falls back to null when there is no value and no selectedValues', () => {
            const property = new Template({ properties: [buildProperty({ name: 'name', value: undefined })] }).properties[0];

            const control = service.createFormControl(property);

            expect(control.value).toBeNull();
        });

        it('applies the required validator', () => {
            const property = new Template({ properties: [buildProperty({ name: 'name', value: '', required: true })] }).properties[0];

            const control = service.createFormControl(property) as FormControl;

            expect(control.hasValidator(Validators.required)).toBe(true);
            expect(control.valid).toBe(false);
        });

        it('does not apply the required validator when not required', () => {
            const property = new Template({ properties: [buildProperty({ name: 'name', value: '' })] }).properties[0];

            const control = service.createFormControl(property) as FormControl;

            expect(control.hasValidator(Validators.required)).toBe(false);
        });

        it('applies the email validator for PropertyType.Email', () => {
            const validProperty = new Template({
                properties: [buildProperty({ name: 'email', type: PropertyType.Email, value: 'foo@example.com' })],
            }).properties[0];
            const invalidProperty = new Template({
                properties: [buildProperty({ name: 'email', type: PropertyType.Email, value: 'not-an-email' })],
            }).properties[0];

            const validControl = service.createFormControl(validProperty) as FormControl;
            const invalidControl = service.createFormControl(invalidProperty) as FormControl;

            expect(validControl.valid).toBe(true);
            expect(invalidControl.valid).toBe(false);
        });

        it('applies min and max validators for numeric properties', () => {
            const property = new Template({
                properties: [buildProperty({ name: 'age', type: PropertyType.Number, min: 5, max: 10, value: 20 })],
            }).properties[0];

            const control = service.createFormControl(property) as FormControl;

            expect(control.valid).toBe(false);
            expect(control.errors).toHaveProperty('max');
        });

        it('applies minLength and maxLength validators for text properties', () => {
            const tooShort = new Template({
                properties: [buildProperty({ name: 'code', minLength: 3, maxLength: 5, value: 'ab' })],
            }).properties[0];
            const tooLong = new Template({
                properties: [buildProperty({ name: 'code', minLength: 3, maxLength: 5, value: 'abcdef' })],
            }).properties[0];
            const justRight = new Template({
                properties: [buildProperty({ name: 'code', minLength: 3, maxLength: 5, value: 'abc' })],
            }).properties[0];

            expect((service.createFormControl(tooShort) as FormControl).valid).toBe(false);
            expect((service.createFormControl(tooLong) as FormControl).valid).toBe(false);
            expect((service.createFormControl(justRight) as FormControl).valid).toBe(true);
        });

        it('applies a pattern validator when a regex is given', () => {
            const validProperty = new Template({
                properties: [buildProperty({ name: 'code', regex: '^[a-z]+$', value: 'abc' })],
            }).properties[0];
            const invalidProperty = new Template({
                properties: [buildProperty({ name: 'code', regex: '^[a-z]+$', value: 'ABC' })],
            }).properties[0];

            expect((service.createFormControl(validProperty) as FormControl).valid).toBe(true);
            expect((service.createFormControl(invalidProperty) as FormControl).valid).toBe(false);
        });
    });

    describe('createFormControl / Object property type', () => {
        it('creates a FormGroup from the default template', () => {
            const property = new Template({
                properties: [buildProperty({
                    name: 'address',
                    type: PropertyType.Object,
                    value: null,
                    _templates: { default: { properties: [buildProperty({ name: 'street', value: 'Main St' })] } },
                })],
            }).properties[0];

            const control = service.createFormControl(property);

            expect(control).toBeInstanceOf(FormGroup);
            expect((control as FormGroup).get('street')?.value).toBe('Main St');
        });

        it('throws when the Object property has no default template', () => {
            const property = new Template({
                properties: [buildProperty({ name: 'address', type: PropertyType.Object, value: null })],
            }).properties[0];

            expect(() => service.createFormControl(property)).toThrow(/no default template/);
        });
    });

    describe('createFormControl / Collection property type', () => {
        it('creates a FormArray from the indexed templates, ignoring the default template', () => {
            const property = new Template({
                properties: [buildProperty({
                    name: 'items',
                    type: PropertyType.Collection,
                    value: null,
                    _templates: {
                        default: { properties: [buildProperty({ name: 'name', value: '' })] },
                        '0': { properties: [buildProperty({ name: 'name', value: 'Item A' })] },
                        '1': { properties: [buildProperty({ name: 'name', value: 'Item B' })] },
                    },
                })],
            }).properties[0];

            const control = service.createFormControl(property);

            expect(control).toBeInstanceOf(FormArray);
            const array = control as FormArray;
            expect(array.length).toBe(2);
            expect(array.at(0).get('name')?.value).toBe('Item A');
            expect(array.at(1).get('name')?.value).toBe('Item B');
        });

        it('produces an empty FormArray when the Collection property has no indexed templates', () => {
            // Note: Property's constructor always defaults _templates to {} (never undefined/null),
            // so the "no templates" guard in FormService never actually throws in practice; instead
            // Object.entries({}) yields no entries and an empty FormArray is produced.
            const property = new Template({
                properties: [buildProperty({ name: 'items', type: PropertyType.Collection, value: null })],
            }).properties[0];

            const control = service.createFormControl(property);

            expect(control).toBeInstanceOf(FormArray);
            expect((control as FormArray).length).toBe(0);
        });
    });

    describe('createFormGroupFromTemplate', () => {
        it('creates a FormGroup with a control for every property', () => {
            const template = buildTemplate([
                buildProperty({ name: 'name', value: 'John' }),
                buildProperty({ name: 'age', type: PropertyType.Number, value: 30 }),
            ]);

            const group = service.createFormGroupFromTemplate(template);

            expect(group).toBeInstanceOf(FormGroup);
            expect(group.get('name')?.value).toBe('John');
            expect(group.get('age')?.value).toBe(30);
        });
    });

    describe('createFormGroupsFromTemplates', () => {
        it('creates a FormGroup for every template', () => {
            const templates: Templates = {
                default: new Template({ properties: [buildProperty({ name: 'name', value: 'John' })] }),
                search: new Template({ properties: [buildProperty({ name: 'query', value: '' })] }),
            };

            const groups = service.createFormGroupsFromTemplates(templates);

            expect(Object.keys(groups)).toEqual(['default', 'search']);
            expect(groups['default'].get('name')?.value).toBe('John');
            expect(groups['search'].get('query')?.value).toBe('');
        });

        it('skips null or undefined templates', () => {
            const templates: Templates = {
                default: new Template({ properties: [] }),
                missing: undefined,
            };

            const groups = service.createFormGroupsFromTemplates(templates);

            expect(Object.keys(groups)).toEqual(['default']);
        });
    });

    describe('createFormArrayFromTemplates', () => {
        it('creates a FormGroup entry for every template, excluding ignored keys', () => {
            const templates: NumberTemplates = {
                default: new Template({ properties: [buildProperty({ name: 'name', value: '' })] }) as unknown as NumberTemplate,
                '0': new NumberTemplate({ properties: [buildProperty({ name: 'name', value: 'Item A' })], title: '0' }),
            };

            const array = service.createFormArrayFromTemplates(templates, ['default']);

            expect(array).toBeInstanceOf(FormArray);
            expect(array.length).toBe(1);
            expect(array.at(0).get('name')?.value).toBe('Item A');
        });

        it('includes all templates when no properties are ignored', () => {
            const templates: NumberTemplates = {
                '0': new NumberTemplate({ properties: [buildProperty({ name: 'name', value: 'Item A' })], title: '0' }),
                '1': new NumberTemplate({ properties: [buildProperty({ name: 'name', value: 'Item B' })], title: '1' }),
            };

            const array = service.createFormArrayFromTemplates(templates);

            expect(array.length).toBe(2);
        });
    });
});
