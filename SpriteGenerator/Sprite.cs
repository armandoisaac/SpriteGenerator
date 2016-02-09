using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using SpriteGenerator.Utility;

namespace SpriteGenerator
{
    internal class Sprite
    {
        private Dictionary<int, string> _cssClassNames;
        private Dictionary<int, Image> _images;
        private readonly LayoutProperties _layoutProp;
        private readonly ILogger _logger;

        public Sprite(LayoutProperties layoutProp, ILogger logger)
        {
            _images = new Dictionary<int, Image>();
            _cssClassNames = new Dictionary<int, string>();
            this._layoutProp = layoutProp;

            // Clear the log
            _logger = logger;
            _logger.Clear();
        }

        public void Create()
        {
            var start = DateTime.Now;
            _logger.Log("Loading image data");
            GetData();
            _logger.Log("... done", true);

            using (var cssFile = File.CreateText(_layoutProp.OutputCssFilePath))
            {
                Image resultSprite = null;

                _logger.Log("Creating css file");
                cssFile.WriteLine("." + _layoutProp.CssPrefix + " { background-image: url('" +
                                  RelativeSpriteImagePath(_layoutProp.OutputSpriteFilePath, _layoutProp.OutputCssFilePath) +
                                  "'); background-color: transparent; background-repeat: no-repeat; }");
                _logger.Log("... done", true);


                switch (_layoutProp.Layout)
                {
                    case "Automatic":
                        resultSprite = GenerateAutomaticLayout(cssFile);
                        break;
                    case "Horizontal":
                        resultSprite = GenerateHorizontalLayout(cssFile);
                        break;
                    case "Vertical":
                        resultSprite = GenerateVerticalLayout(cssFile);
                        break;
                    case "Rectangular":
                        resultSprite = GenerateRectangularLayout(cssFile);
                        break;
                }
                _logger.Log("... done", true);

                if (resultSprite != null)
                    resultSprite.Save(_layoutProp.OutputSpriteFilePath, ImageFormat.Png);
            }

            var elapsed = DateTime.Now - start;
            _logger.Log("Sprite generation complete. Took {0:0.00} seconds.", true, elapsed.TotalSeconds);
        }

        /// <summary>
        ///     Creates dictionary of _images from the given paths and dictionary of CSS classnames from the image filenames.
        /// </summary>
        /// <param name="inputFilePaths">Array of input file paths.</param>
        /// <param name="images">Dictionary of _images to be inserted into the output sprite.</param>
        /// <param name="cssClassNames">Dictionary of CSS classnames.</param>
        private void GetData()
        {
            _images = new Dictionary<int, Image>();
            _cssClassNames = new Dictionary<int, string>();

            var total = _layoutProp.InputFilePaths.Length;
            var step = Math.Ceiling(total * .1);
            for (var i = 0; i < total; i++)
            {
                var img = Image.FromFile(_layoutProp.InputFilePaths[i]);
                _images.Add(i, img);
                var splittedFilePath = _layoutProp.InputFilePaths[i].Split('\\');
                _cssClassNames.Add(i, splittedFilePath[splittedFilePath.Length - 1].Split('.')[0]);

                if (i % step == 0)
                {
                    _logger.Log("... {0:0.00}%", ((double)i / (double)total) * 100);
                }
            }
        }

        private List<Module> CreateModules()
        {
            var modules = new List<Module>();
            foreach (var i in _images.Keys)
                modules.Add(new Module(i, _images[i], _layoutProp.DistanceBetweenImages));
            return modules;
        }

        //CSS line
        private string CssLine(string cssClassName, Rectangle rectangle)
        {
            var line = "." + _layoutProp.CssPrefix + "-" + cssClassName + " { width: " + rectangle.Width + "px; height: " + rectangle.Height +
                       "px; background-position: " + -1 * rectangle.X + "px " + -1 * rectangle.Y + "px; }";
            return line;
        }

        //Relative sprite image file path
        private string RelativeSpriteImagePath(string outputSpriteFilePath, string outputCssFilePath)
        {
            var splittedOutputCssFilePath = outputCssFilePath.Split('\\');
            var splittedOutputSpriteFilePath = outputSpriteFilePath.Split('\\');

            var breakAt = 0;
            for (var i = 0; i < splittedOutputCssFilePath.Length; i++)
                if (i < splittedOutputSpriteFilePath.Length &&
                    splittedOutputCssFilePath[i] != splittedOutputSpriteFilePath[i])
                {
                    breakAt = i;
                    break;
                }

            var relativePath = "";
            for (var i = 0; i < splittedOutputCssFilePath.Length - breakAt - 1; i++)
                relativePath += "../";
            relativePath += string.Join("/", splittedOutputSpriteFilePath, breakAt,
                splittedOutputSpriteFilePath.Length - breakAt);

            return relativePath;
        }

        //Automatic Layout
        private Image GenerateAutomaticLayout(StreamWriter cssFile)
        {
            var sortedByArea = from m in CreateModules()
                               orderby m.Width * m.Height descending
                               select m;
            var moduleList = sortedByArea.ToList();

            _logger.Log("Calculating placement for each image");
            var placement = Algorithm.Greedy(moduleList, _logger);
            _logger.Log("... done", true);

            //Creating an empty result image.
            Image resultSprite =
                new Bitmap(placement.Width - _layoutProp.DistanceBetweenImages + 2 * _layoutProp.MarginWidth,
                    placement.Height - _layoutProp.DistanceBetweenImages + 2 * _layoutProp.MarginWidth);
            var graphics = Graphics.FromImage(resultSprite);

            //Drawing _images into the result image in the original order and writing CSS lines.
            var i = 0;
            var total = placement.Modules.Count;
            var step = Math.Ceiling(placement.Modules.Count * .1);

            _logger.Log("Merging images into a single sprite");
            foreach (var m in placement.Modules)
            {
                m.Draw(graphics, _layoutProp.MarginWidth);
                var rectangle = new Rectangle(m.X + _layoutProp.MarginWidth, m.Y + _layoutProp.MarginWidth,
                    m.Width - _layoutProp.DistanceBetweenImages, m.Height - _layoutProp.DistanceBetweenImages);
                cssFile.WriteLine(CssLine(_cssClassNames[m.Name], rectangle));

                i++;
                if (i % step == 0)
                {
                    _logger.Log("... {0:0.00}%", ((double)i / (double)total) * 100);
                }
            }

            return resultSprite;
        }

        //Horizontal Layout
        private Image GenerateHorizontalLayout(StreamWriter cssFile)
        {
            //Calculating result image dimension.
            var width = 0;
            foreach (var image in _images.Values)
                width += image.Width + _layoutProp.DistanceBetweenImages;
            width = width - _layoutProp.DistanceBetweenImages + 2 * _layoutProp.MarginWidth;
            var height = _images[0].Height + 2 * _layoutProp.MarginWidth;

            //Creating an empty result image.
            Image resultSprite = new Bitmap(width, height);
            var graphics = Graphics.FromImage(resultSprite);

            //Initial coordinates.
            var actualXCoordinate = _layoutProp.MarginWidth;
            var yCoordinate = _layoutProp.MarginWidth;

            //Drawing _images into the result image, writing CSS lines and increasing X coordinate.
            var i = 0;
            var total = _images.Keys.Count;
            var step = Math.Ceiling(total * .1);
            _logger.Log("Merging images into a single sprite");
            foreach (var key in _images.Keys)
            {
                var rectangle = new Rectangle(actualXCoordinate, yCoordinate, _images[key].Width, _images[key].Height);
                graphics.DrawImage(_images[key], rectangle);
                cssFile.WriteLine(CssLine(_cssClassNames[key], rectangle));
                actualXCoordinate += _images[key].Width + _layoutProp.DistanceBetweenImages;

                i++;
                if (i % step == 0)
                {
                    _logger.Log("... {0:0.00}%", ((double)i / (double)total) * 100);
                }
            }

            return resultSprite;
        }

        //Vertical Layout
        private Image GenerateVerticalLayout(StreamWriter cssFile)
        {
            //Calculating result image dimension.
            var height = 0;
            foreach (var image in _images.Values)
                height += image.Height + _layoutProp.DistanceBetweenImages;
            height = height - _layoutProp.DistanceBetweenImages + 2 * _layoutProp.MarginWidth;
            var width = _images[0].Width + 2 * _layoutProp.MarginWidth;

            //Creating an empty result image.
            Image resultSprite = new Bitmap(width, height);
            var graphics = Graphics.FromImage(resultSprite);

            //Initial coordinates.
            var actualYCoordinate = _layoutProp.MarginWidth;
            var xCoordinate = _layoutProp.MarginWidth;

            //Drawing _images into the result image, writing CSS lines and increasing Y coordinate.
            var i = 0;
            var total = _images.Keys.Count;
            var step = Math.Ceiling(total * .1);
            _logger.Log("Merging images into a single sprite");
            foreach (var key in _images.Keys)
            {
                var rectangle = new Rectangle(xCoordinate, actualYCoordinate, _images[key].Width, _images[key].Height);
                graphics.DrawImage(_images[key], rectangle);
                cssFile.WriteLine(CssLine(_cssClassNames[key], rectangle));
                actualYCoordinate += _images[key].Height + _layoutProp.DistanceBetweenImages;

                i++;
                if (i % step == 0)
                {
                    _logger.Log("... {0:0.00}%", ((double)i / (double)total) * 100);
                }
            }

            return resultSprite;
        }

        private Image GenerateRectangularLayout(StreamWriter cssFile)
        {
            //Calculating result image dimension.
            var imageWidth = _images[0].Width;
            var imageHeight = _images[0].Height;
            var width = _layoutProp.ImagesInRow * (imageWidth + _layoutProp.DistanceBetweenImages) -
                        _layoutProp.DistanceBetweenImages + 2 * _layoutProp.MarginWidth;
            var height = _layoutProp.ImagesInColumn * (imageHeight + _layoutProp.DistanceBetweenImages) -
                         _layoutProp.DistanceBetweenImages + 2 * _layoutProp.MarginWidth;

            //Creating an empty result image.
            Image resultSprite = new Bitmap(width, height);
            var graphics = Graphics.FromImage(resultSprite);

            //Initial coordinates.
            var actualYCoordinate = _layoutProp.MarginWidth;
            var actualXCoordinate = _layoutProp.MarginWidth;

            //Drawing _images into the result image, writing CSS lines and increasing coordinates.
            var countAux = 0;
            var total = _images.Keys.Count;
            var step = Math.Ceiling(total * .1);
            _logger.Log("Merging images into a single sprite");
            for (var i = 0; i < _layoutProp.ImagesInColumn; i++)
            {
                for (var j = 0; i * _layoutProp.ImagesInRow + j < _images.Count && j < _layoutProp.ImagesInRow; j++)
                {
                    var rectangle = new Rectangle(actualXCoordinate, actualYCoordinate, imageWidth, imageHeight);
                    graphics.DrawImage(_images[i * _layoutProp.ImagesInRow + j], rectangle);
                    cssFile.WriteLine(CssLine(_cssClassNames[i * _layoutProp.ImagesInRow + j], rectangle));
                    actualXCoordinate += imageWidth + _layoutProp.DistanceBetweenImages;
                    countAux++;

                    if (countAux % step == 0)
                    {
                        _logger.Log("... {0:0.00}%", ((double)countAux / (double)total) * 100);
                    }
                }
                actualYCoordinate += imageHeight + _layoutProp.DistanceBetweenImages;
                actualXCoordinate = _layoutProp.MarginWidth;
            }

            return resultSprite;
        }
    }
}