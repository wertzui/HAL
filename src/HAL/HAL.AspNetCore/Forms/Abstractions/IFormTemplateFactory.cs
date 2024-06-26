﻿using HAL.Common;
using HAL.Common.Forms;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HAL.AspNetCore.Forms.Abstractions;

/// <summary>
/// A factory that will create form templates for a given type.
/// You probably want to use <see cref="IFormFactory"/> instead.
/// </summary>
public interface IFormTemplateFactory
{
    /// <summary>
    /// Creates a form for the given type.
    /// </summary>
    /// <param name="dtoType">The type to create a form for.</param>
    /// <param name="method">The method that is used when the form is submitted.</param>
    /// <param name="title">A human readable title for the form.</param>
    /// <param name="contentType">The content type that is used when the form is submitted.</param>
    /// <returns>A HAL form for the given type, but without any values.</returns>
    ValueTask<FormTemplate> CreateTemplateForAsync(Type dtoType, string method, string? title = null, string contentType = Constants.MediaTypes.Json);

    /// <summary>
    /// Creates a form for the given type.
    /// </summary>
    /// <typeparam name="TDto">The type to create a form for.</typeparam>
    /// <param name="method">The method that is used when the form is submitted.</param>
    /// <param name="title">A human readable title for the form.</param>
    /// <param name="contentType">The content type that is used when the form is submitted.</param>
    /// <returns>A HAL form for the given type, but without any values.</returns>
    ValueTask<FormTemplate> CreateTemplateForAsync<TDto>(string method, string? title = null, string contentType = Constants.MediaTypes.Json);

    /// <summary>
    /// Creates a template collection with the "default" template for the given type.
    /// </summary>
    /// <param name="dtoType">The type to create a form for.</param>
    /// <param name="method">The method that is used when the form is submitted.</param>
    /// <param name="title">A human readable title for the form.</param>
    /// <param name="contentType">The content type that is used when the form is submitted.</param>
    /// <returns>A new dictionary with only a "default" form template.</returns>
    ValueTask<IDictionary<string, FormTemplate>> CreateTemplatesWithDefaultEntryAsync(Type dtoType, string? method, string? title = null, string contentType = "application/json");
}