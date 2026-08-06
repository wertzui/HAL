import { Service } from "@angular/core";
import { FormArray, FormControl, FormGroup, Validators } from '@angular/forms';
import { NumberTemplates, Property, PropertyDto, PropertyType, SimpleValue, TemplateBase, Templates } from "../models/formsResource";

/**
 * The shape of a form group's controls, as produced by {@link FormService.createFormGroupFromTemplate}.
 */
type FormGroupControls = { [key: string]: FormControlOrGroupOrArray };

/**
 * The type of value returned by {@link FormService.createFormControl}. It mirrors the possible
 * shapes of a form built from a HAL-FORMS template: a plain control for simple properties, a
 * group of controls for `Object` properties, or an array of groups for `Collection` properties.
 * Each control inside a group or array is, recursively, one of these same three shapes.
 *
 * This is declared as an explicit, named, recursive type alias rather than being derived via
 * `ReturnType<FormService["..."]>`. `createFormGroupFromTemplate` and `createFormControl`
 * reference each other's return type; if both return types were themselves inferred purely from
 * `ReturnType<FormService["otherMethod"]>`, TypeScript cannot resolve that cycle and reports
 * TS2577 ("Return type annotation circularly references itself"). A named recursive type alias
 * like this one does not have that limitation.
 */
export type FormControlOrGroupOrArray<TValue extends SimpleValue = SimpleValue> =
  | FormGroup<FormGroupControls>
  | FormArray<FormGroup<FormGroupControls>>
  | FormControl<TValue | null | TValue[]>;

/**
 * A service that provides methods for creating form groups and form controls from templates and properties.
 */
@Service()
export class FormService {

  /**
   * Creates form groups from templates.
   * @param templates The templates to create form groups from.
   * @returns An object containing the form groups created from the templates.
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
   * const formGroups = this.formService.createFormGroupsFromTemplates(templates);
   * ```
   */
  public createFormGroupsFromTemplates(templates: Templates): { [key: string]: FormGroup<FormGroupControls> } {
    const tabs = Object.fromEntries(
      Object.entries(templates)
        .filter(([_, template]) => template !== undefined && template !== null)
        .map(([name, template]) => [
          name,
          this.createFormGroupFromTemplate(template!)
        ]));

    return tabs;
  }

  /**
   * Creates a FormArray from the given templates, while ignoring the specified properties.
   * @param templates - The templates to create the FormArray from.
   * @param ignoredProperties - The properties to ignore while creating the FormArray. This is useful for ignoring the "default" template, for example.
   * @returns A FormArray of the specified type.
   * 
   * @example
   * ```typescript
   * const templates: Templates = {
   *   default: {
   *     title: 0,
   *     properties: [{
   *      name: 'name',
   *      type: PropertyType.String,
   *      value: 'John Doe',
   *   }]}
   * };
   * 
   * const formArray = this.formService.createFormArrayFromTemplates(templates);
   * ```
   */
  public createFormArrayFromTemplates(templates: NumberTemplates, ignoredProperties: string[] = []): FormArray<FormGroup<FormGroupControls>> {
    const controls =
      Object.entries(templates)
        .filter(([key,]) => !ignoredProperties.some(p => key === p))
        .map(([, template]) =>
          this.createFormGroupFromTemplate(template!));
    const formArray = new FormArray(controls);
    return formArray;
  }

  /**
   * Creates a new FormGroup instance based on the provided template.
   * @param template The template to use for creating the form group.
   * @returns A new FormGroup instance.
   * 
   * 
   * @example
   * ```typescript
   * const template: Template = {
   * properties: [{
   *   name: 'name',
   *   type: PropertyType.String,
   *   value: 'John Doe',
   * }]};
   * 
   * const formGroup = this.formService.createFormGroupFromTemplate(template);
   * ```
   */
  public createFormGroupFromTemplate<TProperties extends ReadonlyArray<PropertyDto<SimpleValue, string, string>>>(template: TemplateBase<string | number, TProperties>): FormGroup<FormGroupControls> {
    const controls = Object.fromEntries(template.properties.map(p => [
      p.name,
      this.createFormControl(p)
    ]));
    const formGroup = new FormGroup(controls);
    return formGroup;
  }

  /**
   * Creates a form control based on the given property.
   * @param property The property to create the form control for.
   * @returns The created form control.
   * 
   * @remarks
   * This method will create a form control based on the type of the property.
   * If the property is of type `Object`, it will create a form group from the default template of the property.
   * If the property is of type `Collection`, it will create a form array from the templates of the property.
   * Otherwise, it will create a simple form control from the property with validators as specified in the property.
   * 
   * 
   * 
   * @example
   * ```typescript
   * const property: Property = {
   *  name: 'name',
   *  type: PropertyType.String,
   *  value: 'John Doe',
   * };
   * 
   * const formControl = this.formService.createFormControl(property);
   * ```
   */
  public createFormControl<
      TValue extends SimpleValue = SimpleValue, 
      OptionsPromptField extends string = "prompt", 
      OptionsValueField extends string = "value">(
    property: Property<TValue, OptionsPromptField, OptionsValueField>)
    : FormControlOrGroupOrArray<TValue> {
    if (property.type === PropertyType.Object) {
      const defaultTemplate = property._templates['default'];
      if (!defaultTemplate)
        throw new Error(`The property ${property.name} is of type Object, but has no default template.`);
      return this.createFormGroupFromTemplate(defaultTemplate);
    }

    if (property.type === PropertyType.Collection) {
      if (!property._templates)
        throw new Error(`The property ${property.name} is of type Collection, but has no templates.`);
      return this.createFormArrayFromTemplates(property._templates as NumberTemplates, ['default']);
    }

    return this.createSimpleFormControlFromProperty(property);
  }

  private createSimpleFormControlFromProperty<
      TValue extends SimpleValue = SimpleValue, 
      OptionsPromptField extends string = "prompt", 
      OptionsValueField extends string = "value">(
    property: Property<TValue, OptionsPromptField, OptionsValueField>)
    : FormControl<TValue | null | TValue[]> {
    const value = FormService.getPropertyValue(property);
    const control = new FormControl(value);

    if (property.max !== undefined && property.max !== null)
      control.addValidators(Validators.max(property.max));
    if (property.maxLength !== undefined && property.maxLength !== null)
      control.addValidators(Validators.maxLength(property.maxLength));
    if (property.min !== undefined && property.min !== null)
      control.addValidators(Validators.min(property.min));
    if (property.minLength !== undefined && property.minLength !== null)
      control.addValidators(Validators.minLength(property.minLength));
    if (property.regex)
      control.addValidators(Validators.pattern(property.regex));
    if (property.required)
      control.addValidators(Validators.required);
    if (property.type === PropertyType.Email)
      control.addValidators(Validators.email);

    // Validators added via addValidators() are not automatically applied to the control's
    // current value/status, so we need to explicitly trigger a re-validation here.
    control.updateValueAndValidity();

    return control;
  }

  private static getPropertyValue<TValue extends SimpleValue = SimpleValue>(property: Property<TValue, string, string>): TValue | null | TValue[]{
    if (property.options && Array.isArray(property.options.selectedValues) && property.options.selectedValues.length > 0) {
      if (property.options.maxItems !== undefined && property.options.maxItems > 1)
        return property.options.selectedValues;

      return property.options.selectedValues[0];
    }

    return property.value ?? null;
  }
}
