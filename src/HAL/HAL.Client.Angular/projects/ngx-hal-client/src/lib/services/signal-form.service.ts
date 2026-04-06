import { Injectable, signal, WritableSignal } from "@angular/core";
import { form, required, email, min, max, minLength, maxLength, pattern, applyEach, SchemaPath, SchemaPathTree, FieldTree } from '@angular/forms/signals';
import { Property, PropertyDto, PropertyType, SimpleValue, TemplateBase, Templates, NumberTemplates } from "../models/formsResource";

/**
 * Wraps the signal model and the form field tree created by Signal Forms.
 * @template T The type of the form model.
 */
export interface SignalForm<T extends Record<string, unknown> = Record<string, unknown>> {
  /** The writable signal holding the form model data. Use `model()` to read, `model.set()` to replace. */
  model: WritableSignal<T>;
  /** The field tree created by `form()`. Use it to bind to `[formField]` directives and access field state. */
  form: FieldTree<T>;
}

@Injectable({
  providedIn: 'root'
})
/**
 * A service that provides methods for creating Signal Forms from templates and properties.
 * This is the Signal Forms equivalent of {@link FormService}.
 */
export class SignalFormService {

  /**
   * Creates signal forms from templates.
   * @param templates The templates to create signal forms from.
   * @param model An optional model whose values are used instead of the template property values. Each key corresponds to a template name.
   * @returns An object containing the signal forms created from the templates.
   *
   * @example
   * ```typescript
   * const templates: Templates = {
   *  default: {
   *   properties: [{
   *    name: 'name',
   *    type: PropertyType.String,
   *    value: 'John Doe',
   * }]}};
   *
   * // Without model (values from template)
   * const signalForms = this.signalFormService.createSignalFormsFromTemplates(templates);
   *
   * // With model (values from model)
   * const signalForms = this.signalFormService.createSignalFormsFromTemplates(templates, { default: { name: 'Jane' } });
   * ```
   */
  public createSignalFormsFromTemplates<TModel extends Record<string, Record<string, unknown>>>(templates: Templates, model: TModel): { [K in keyof TModel]: SignalForm<TModel[K]> };
  public createSignalFormsFromTemplates(templates: Templates): { [key: string]: SignalForm };
  public createSignalFormsFromTemplates(templates: Templates, model?: Record<string, Record<string, unknown>>): { [key: string]: SignalForm } {
    const tabs = Object.fromEntries(
      Object.entries(templates)
        .filter(([_, template]) => template !== undefined && template !== null)
        .map(([name, template]) => {
          const templateModel = model?.[name];
          return [
            name,
            templateModel !== undefined
              ? this.createSignalFormFromTemplate(template!, templateModel)
              : this.createSignalFormFromTemplate(template!)
          ];
        }));

    return tabs;
  }

  /**
   * Creates an array of signal forms from the given templates, while ignoring the specified properties.
   * When a model array is passed, the default template is used for every entry and the values come from the model array.
   * @param templates - The templates to create the signal forms from.
   * @param ignoredProperties - The properties to ignore while creating the signal forms. This is useful for ignoring the "default" template, for example.
   * @param model - An optional array of model objects whose values are used instead of the template property values.
   * @returns An array of SignalForm objects.
   *
   * @example
   * ```typescript
   * const templates: NumberTemplates = {
   *   default: { properties: [{ name: 'name', type: PropertyType.String, value: '' }] },
   *   0: { title: 0, properties: [{ name: 'name', type: PropertyType.String, value: 'John' }] },
   * };
   *
   * // Without model: one form per numbered template
   * const signalForms = this.signalFormService.createSignalFormArrayFromTemplates(templates);
   *
   * // With model: one form per model entry, all using the default template
   * const signalForms = this.signalFormService.createSignalFormArrayFromTemplates(templates, [], [{ name: 'Jane' }, { name: 'Bob' }]);
   * ```
   */
  public createSignalFormArrayFromTemplates<TItem extends Record<string, unknown>>(templates: NumberTemplates, ignoredProperties: string[], model: TItem[]): SignalForm<TItem>[];
  public createSignalFormArrayFromTemplates(templates: NumberTemplates, ignoredProperties?: string[]): SignalForm[];
  public createSignalFormArrayFromTemplates(templates: NumberTemplates, ignoredProperties: string[] = [], model?: Record<string, unknown>[]): SignalForm[] {
    if (model) {
      const defaultTemplate = templates['default'];
      if (!defaultTemplate)
        throw new Error('A model array was provided, but the templates have no "default" template to use as the structure.');
      return model.map(itemModel =>
        this.createSignalFormFromTemplate(defaultTemplate, itemModel));
    }

    return Object.entries(templates)
      .filter(([key,]) => !ignoredProperties.some(p => key === p))
      .map(([, template]) =>
        this.createSignalFormFromTemplate(template!));
  }

  /**
   * Creates a new SignalForm instance based on the provided template.
   * @param template The template to use for creating the signal form.
   * @param model An optional model object whose values are used instead of the template property values.
   * @returns A new SignalForm instance containing the model signal and form field tree.
   *
   * @example
   * ```typescript
   * const template: Template = {
   *   properties: [{
   *     name: 'name',
   *     type: PropertyType.String,
   *     value: 'John Doe',
   *   }]
   * };
   *
   * // Without model
   * const signalForm = this.signalFormService.createSignalFormFromTemplate(template);
   *
   * // With model
   * const signalForm = this.signalFormService.createSignalFormFromTemplate(template, { name: 'Jane' });
   * ```
   */
  public createSignalFormFromTemplate<TModel extends Record<string, unknown>, TProperties extends ReadonlyArray<PropertyDto<SimpleValue, string, string>>>(template: TemplateBase<string | number, TProperties>, model: TModel): SignalForm<TModel>;
  public createSignalFormFromTemplate<TProperties extends ReadonlyArray<PropertyDto<SimpleValue, string, string>>>(template: TemplateBase<string | number, TProperties>): SignalForm;
  public createSignalFormFromTemplate<TProperties extends ReadonlyArray<PropertyDto<SimpleValue, string, string>>>(
    template: TemplateBase<string | number, TProperties>,
    model?: Record<string, unknown>
  ): SignalForm {
    const modelValue = model ?? this.buildModelFromTemplate(template);
    const modelSignal = signal(modelValue);
    const signalForm = form(modelSignal, (schemaPath: SchemaPathTree<Record<string, unknown>>) => {
      this.applyValidationFromTemplate(template, schemaPath);
    });

    return { model: modelSignal, form: signalForm };
  }

  private buildModelFromTemplate<TProperties extends ReadonlyArray<PropertyDto<SimpleValue, string, string>>>(
    template: TemplateBase<string | number, TProperties>
  ): Record<string, unknown> {
    const model: Record<string, unknown> = {};
    for (const property of template.properties) {
      model[property.name] = this.getPropertyModelValue(property);
    }
    return model;
  }

  private getPropertyModelValue(property: Property<SimpleValue, string, string>): unknown {
    if (property.type === PropertyType.Object) {
      const defaultTemplate = property._templates['default'];
      if (!defaultTemplate)
        throw new Error(`The property ${property.name} is of type Object, but has no default template.`);
      return this.buildModelFromTemplate(defaultTemplate);
    }

    if (property.type === PropertyType.Collection) {
      if (!property._templates)
        throw new Error(`The property ${property.name} is of type Collection, but has no templates.`);
      return Object.entries(property._templates)
        .filter(([key,]) => key !== 'default')
        .map(([, template]) => this.buildModelFromTemplate(template!));
    }

    return SignalFormService.getSimplePropertyValue(property);
  }

  private static getSimplePropertyValue(property: Property<SimpleValue, string, string>): SimpleValue | SimpleValue[] {
    if (property.options && Array.isArray(property.options.selectedValues) && property.options.selectedValues.length > 0) {
      if (property.options.maxItems !== undefined && property.options.maxItems > 1)
        return property.options.selectedValues;

      return property.options.selectedValues[0];
    }

    return property.value ?? null;
  }

  private applyValidationFromTemplate<TProperties extends ReadonlyArray<PropertyDto<SimpleValue, string, string>>>(
    template: TemplateBase<string | number, TProperties>,
    schemaPath: SchemaPathTree<Record<string, unknown>>
  ): void {
    for (const property of template.properties) {
      const fieldPath = schemaPath[property.name];
      if (!fieldPath) continue;

      this.applyValidationFromProperty(property, fieldPath);
    }
  }

  private applyValidationFromProperty(property: Property<SimpleValue, string, string>, fieldPath: SchemaPath<unknown>): void {
    if (property.type === PropertyType.Object) {
      const defaultTemplate = property._templates['default'];
      if (defaultTemplate) {
        const objectPath = fieldPath as SchemaPathTree<Record<string, unknown>>;
        for (const nestedProp of defaultTemplate.properties) {
          const nestedPath = objectPath[nestedProp.name];
          if (nestedPath) {
            this.applyValidationFromProperty(nestedProp, nestedPath);
          }
        }
      }
      return;
    }

    if (property.type === PropertyType.Collection) {
      const defaultTemplate = property._templates['default'];
      if (defaultTemplate) {
        const arrayPath = fieldPath as SchemaPath<Record<string, unknown>[]>;
        applyEach(arrayPath, (itemPath: SchemaPathTree<Record<string, unknown>>) => {
          for (const itemProp of defaultTemplate.properties) {
            const itemFieldPath = itemPath[itemProp.name];
            if (itemFieldPath) {
              this.applyValidationFromProperty(itemProp, itemFieldPath);
            }
          }
        });
      }
      return;
    }

    if (property.required)
      required(fieldPath as SchemaPath<SimpleValue>);
    if (property.type === PropertyType.Email)
      email(fieldPath as SchemaPath<string>);
    if (property.max !== undefined)
      max(fieldPath as SchemaPath<number>, property.max);
    if (property.min !== undefined)
      min(fieldPath as SchemaPath<number>, property.min);
    if (property.maxLength !== undefined)
      maxLength(fieldPath as SchemaPath<string>, property.maxLength);
    if (property.minLength !== undefined)
      minLength(fieldPath as SchemaPath<string>, property.minLength);
    if (property.regex)
      pattern(fieldPath as SchemaPath<string>, new RegExp(property.regex));
  }
}
