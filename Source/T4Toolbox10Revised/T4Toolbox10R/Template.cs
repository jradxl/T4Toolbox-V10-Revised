// <copyright file="Template.cs" company="T4 Toolbox Team">
//  Copyright © T4 Toolbox Team. All Rights Reserved.
// </copyright>

namespace T4Toolbox
{
    using Microsoft.VisualStudio.TextTemplating;
    using System;
    using System.CodeDom.Compiler;
    using System.Globalization;

    /// <summary>
    /// Abstract base class for nested template classes.
    /// </summary>
    public abstract class Template : TextTransformation
    {
        #region fields

        /// <summary>
        /// Stores the value that determines whether this <see cref="Template"/> will be rendered.
        /// </summary>
        private bool enabled = true;

        /// <summary>
        /// Stores the object that determines where and how output of this <see cref="Template"/>
        /// will be stored.
        /// </summary>
        private OutputInfo output = new OutputInfo();

        #endregion

        /// <summary>
        /// Occurs directly after <see cref="Render"/> method is called.
        /// </summary>
        /// <remarks>
        /// When implementing a composite <see cref="Generator"/>, use its constructor
        /// to specify an event handler to update the <see cref="Output"/> properties
        /// for individual templates. This will allow users of the generator to change
        /// how output is saved to fit their needs without modifying the generator itself.
        /// </remarks>
        public event EventHandler Rendering;

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Template"/> will be
        /// rendered.
        /// </summary>
        /// <value>
        /// A <see cref="Boolean"/> value. <code>true</code> if <see cref="Template"/> 
        /// will be rendered; otherwise, <code>false</code>. The default is <code>true</code>.
        /// </value>
        /// <remarks>
        /// This property allows users of complex code generators to turn off generation of
        /// a particular output type without having to reimplement the <see cref="Generator"/>.
        /// </remarks>
        public bool Enabled
        {
            get { return this.enabled; }
            set { this.enabled = value; }
        }

        /// <summary>
        /// Gets the collection of errors that occurred during template rendering.
        /// </summary>
        /// <value>
        /// A collection of <see cref="CompilerError"/> objects.
        /// </value>
        /// <remarks>
        /// Use this property when testing error handling logic of your template.
        /// </remarks>
        public new CompilerErrorCollection Errors
        {
            get { return base.Errors; }
        }

        /// <summary>
        /// Gets the object that determines where and how the output of this <see cref="Template"/>
        /// will be saved.
        /// </summary>
        /// <value>
        /// An <see cref="OutputInfo"/> object.
        /// </value>
        /// <remarks>
        /// When implementing a composite <see cref="Generator"/>, use <see cref="Rendering"/>
        /// event to update <see cref="Output"/> properties each time when the <see cref="Template"/>
        /// is rendered.
        /// </remarks>
        public OutputInfo Output
        {
            get { return this.output; }
        }

        /// <summary>
        /// Adds a new error to the list of <see cref="Errors"/> produced by the current template rendering.
        /// </summary>
        /// <param name="format">
        /// A <see cref="string.Format(string, object)"/> string of the error message.
        /// </param>
        /// <param name="args">
        /// An array of one or more <paramref name="format"/> arguments.
        /// </param>
        public void Error(string format, params object[] args)
        {
            base.Error(string.Format(CultureInfo.CurrentCulture, format, args));
        }

        /// <summary>
        /// Transforms the template and saves generated content based on <see cref="Output"/> settings.
        /// </summary>
        public void Render()
        {
            this.OnRendering(EventArgs.Empty);
            if (this.Enabled)
            {
                string content = this.Transform();
                TransformationContext.Render(content, this.Output, this.Errors);
            }
        }

        /// <summary>
        /// Transforms the template and saves generated content based on <see cref="Output"/> settings.
        /// Private, so to not offer this option to Template
        /// </summary>
        /// Suggestion made by ggreig in Nov 2008, with thanks
        private void RenderIfNotExists()
        {
            this.OnRendering(EventArgs.Empty);
            if (this.Enabled)
            {
                string content = this.Transform();
                TransformationContext.RenderIfNotExists(content, this.Output, this.Errors);
            }
        }

        /// <summary>
        /// Transforms the template and saves generated content to the specified file.
        /// </summary>
        /// <param name="fileName">
        /// Name of the output file.
        /// </param>
        public void RenderToFile(string fileName)
        {
            this.Output.File = fileName;
            this.Render();
        }

        /// <summary>
        /// Renders the template and saves its output to the specified file,
        /// only if the file does not already exist.
        /// </summary>
        /// <param name="fileName">
        /// Name of the output file
        /// </param>
        /// Suggestion made by ggreig in Nov 2008, with thanks
        public void RenderToFileIfNotExists(string fileName)
        {
            this.Output.File = fileName;
            this.RenderIfNotExists();
        }

        /// <summary>
        /// Transforms the template into output.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> with content generated by this template.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method emulates behavior of the T4's built-in TransformationRunner. 
        /// It initializes the template, validates, executes it and returns the generated 
        /// content. Unlike the <see cref="Render"/>, this method does not attempt to 
        /// save the generated content. 
        /// </para>
        /// <para>
        /// This method is low-level and intended for use in unit tests that verify 
        /// generated output. Use <see cref="Render"/> method when implementing 
        /// <see cref="Generator"/> code or code generation scripts.
        /// </para>
        /// </remarks>
        public string Transform()
        {
            // Clear results of previous transformation (if any)
            this.Errors.Clear();
            this.GenerationEnvironment.Remove(0, this.GenerationEnvironment.Length);

            try
            {
                // Run code generated by custom directive processors
                this.Initialize();

                // Verify pre-conditions 
                this.Validate();
                if (!this.Errors.HasErrors)
                {
                    // Generate output
                    return this.TransformText();
                }
            }
            catch (TransformationException e)
            {
                // Report expected errors without exception call stack
                this.Error(e.Message);
            }

            return this.GenerationEnvironment.ToString();
        }

        /// <summary>
        /// Adds a new warning to the list of <see cref="Errors"/> produced by the current template rendering.
        /// </summary>
        /// <param name="format">
        /// A <see cref="string.Format(string, object)"/> string of the warning message.
        /// </param>
        /// <param name="args">
        /// An array of one or more <paramref name="format"/> arguments.
        /// </param>
        public void Warning(string format, params object[] args)
        {
            base.Warning(string.Format(CultureInfo.CurrentCulture, format, args));
        }

        /// <summary>
        /// Raises the <see cref="Rendering"/> event.
        /// </summary>
        /// <param name="e">
        /// An <see cref="EventArgs"/> that contains the event data. 
        /// </param>
        protected virtual void OnRendering(EventArgs e)
        {
            if (this.Rendering != null)
            {
                this.Rendering(this, e);
            }
        }

        /// <summary>
        /// When overridden in a derived class, validates parameters of the template.
        /// </summary>
        /// <remarks>
        /// Override this method in derived classes to validate required and optional
        /// parameters of this <see cref="Template"/>. Call <see cref="Error"/>, <see cref="Warning"/> 
        /// methods or throw <see cref="TransformationException"/> to report errors.
        /// </remarks>
        protected virtual void Validate()
        {
            // This method is intentionally left blank
        }
    }
}