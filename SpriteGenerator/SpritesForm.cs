using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpriteGenerator
{
    public partial class SpritesForm : Form
    {
        private readonly LayoutProperties _config = new LayoutProperties();

        public SpritesForm()
        {
            InitializeComponent();
            _config.Layout = radioButtonAutomaticLayout.Text;
        }

        //Generate button click event. Start generating output image and CSS file.
        private void buttonGenerate_Click(object sender, EventArgs e)
        {
            ToggleGenerateButton(false);
            _config.OutputSpriteFilePath = Path.Combine(txtOutputPath.Text, string.Format("{0}.png", txtFileName.Text));
            _config.OutputCssFilePath = Path.Combine(txtOutputPath.Text, string.Format("{0}.css", txtFileName.Text));
            _config.DistanceBetweenImages = (int)numericUpDownDistanceBetweenImages.Value;
            _config.MarginWidth = (int)numericUpDownMarginWidth.Value;
            _config.CssPrefix = txtCssPrefix.Text;

            // Create the output form
            var progressForm = new Progress();
            progressForm.Show();

            Task.Factory.StartNew(() =>
            {
                var sprite = new Sprite(_config, progressForm);
                sprite.Create();
            }).ContinueWith(t =>
            {
                ToggleGenerateButton(true);
            });
        }

        //Browse input images folder.
        private void buttonBrowseFolder_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                string[] filters = { ".png", ".jpg", ".jpeg", ".gif" };
                _config.InputFilePaths = (from filter in filters
                                          from file in Directory.GetFiles(folderBrowserDialog.SelectedPath)
                                          where file.EndsWith(filter)
                                          select file).ToArray();
                var totalImages = _config.InputFilePaths.Length;
                //If there is no file with the enabled formats in the choosen directory.
                if (_config.InputFilePaths.Length == 0)
                {
                    MessageBox.Show("This directory does not contain image files.");
                }
                else
                {
                    txtInputPath.Text = folderBrowserDialog.SelectedPath;

                    radioButtonAutomaticLayout.Checked = true;
                    var width = Image.FromFile(_config.InputFilePaths[0]).Width;
                    var height = Image.FromFile(_config.InputFilePaths[0]).Height;

                    // 
                    var horizontalSame = _config.InputFilePaths.All(file => Image.FromFile(file).Height == height);
                    var verticalSame = _config.InputFilePaths.All(file => Image.FromFile(file).Width == width);

                    //Horizontal Layout radiobutton is enabled only when all image heights are the same.
                    radioButtonHorizontalLayout.Enabled = horizontalSame && totalImages <= 200;

                    //Vertical Layout radiobutton is enabled only when all image widths are the same.
                    radioButtonVerticalLayout.Enabled = verticalSame && totalImages <= 200;

                    //Rectangular Layout radiobutton is enabled only when all image heights and all image widths are the same.
                    radioButtonRectangularLayout.Enabled = horizontalSame && verticalSame;

                    //Setting rectangular Layout dimensions.
                    if (radioButtonRectangularLayout.Enabled)
                    {
                        numericUpDownImagesInRow.Value = (int)Math.Ceiling(Math.Sqrt(_config.InputFilePaths.Length));
                        UpdateImagesPerRow();
                    }
                    else
                    {
                        numericUpDownImagesInRow.Minimum = 0;
                        numericUpDownImagesInColumn.Minimum = 0;
                        numericUpDownImagesInRow.Value = 0;
                        numericUpDownImagesInColumn.Value = 0;
                    }
                }
            }
        }

        //Select output image file path.
        private void buttonSelectOutputImageFilePath_Click(object sender, EventArgs e)
        {
            if (fileDialogSavePath.ShowDialog() == DialogResult.OK)
            {
                txtOutputPath.Text = fileDialogSavePath.SelectedPath;
            }
        }

        //Rectangular Layout radiobutton checked change.
        private void radioButtonRectangularLayout_CheckedChanged(object sender, EventArgs e)
        {
            radioButtonLayout_CheckedChanged(sender, e);
            //Enabling numericupdowns to select Layout dimension.
            if (radioButtonRectangularLayout.Checked)
            {
                numericUpDownImagesInRow.Enabled = true;
                numericUpDownImagesInColumn.Enabled = true;
                labelX.Enabled = true;
                labelSprites.Enabled = true;
                numericUpDownImagesInRow.Maximum = _config.InputFilePaths.Length;
            }

            //Disabling numericupdowns
            else
            {
                numericUpDownImagesInRow.Enabled = false;
                numericUpDownImagesInColumn.Enabled = false;
                labelX.Enabled = false;
                labelSprites.Enabled = false;
            }
        }

        //Checked change event for all Layout radiobutton.
        private void radioButtonLayout_CheckedChanged(object sender, EventArgs e)
        {
            //Setting Layout field value.
            if (((RadioButton)sender).Checked)
                _config.Layout = ((RadioButton)sender).Text;
        }

        //Sprites in row numericupdown value changed event
        private void numericUpDownImagesInRow_ValueChanged(object sender, EventArgs e)
        {
            UpdateImagesPerRow();
        }

        private void UpdateImagesPerRow()
        {
            var numberOfFiles = _config.InputFilePaths.Length;
            //Setting sprites in column numericupdown value
            numericUpDownImagesInColumn.Minimum = numberOfFiles / (int)numericUpDownImagesInRow.Value;
            numericUpDownImagesInColumn.Minimum += numberOfFiles % (int)numericUpDownImagesInRow.Value > 0 ? 1 : 0;
            numericUpDownImagesInColumn.Maximum = numericUpDownImagesInColumn.Minimum;

            _config.ImagesInRow = (int)numericUpDownImagesInRow.Value;
            _config.ImagesInColumn = (int)numericUpDownImagesInColumn.Value;
        }

        private void SpritesForm_Load(object sender, EventArgs e)
        {

        }

        private void ToggleGenerateButton(bool enabled)
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)delegate
                {
                    ToggleGenerateButton(enabled);
                });
                return;
            }

            buttonGenerate.Enabled = enabled;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}