using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.FileSystem;
using MetadataExtractor.Formats.FileType;
using Directory = MetadataExtractor.Directory;

namespace IViewer {
    public class MyMetaDataExtractor
    {
      private IEnumerable<Directory> directories;

        public MyMetaDataExtractor()
        {
          
        }

        public bool IsReaded() {
          return directories == null;
        }
        
        public void ReadPic(string imagePath) {
          if(File.Exists(imagePath))
            directories = ImageMetadataReader.ReadMetadata(imagePath);
        }

        public string SourseOutput() {
          if (directories == null) return null;
          string output = "Source\n";
          var subIfdDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
          var dateTime = subIfdDirectory?.GetDescription(ExifDirectoryBase.TagDateTimeOriginal);
          output += "Date:" + dateTime + "\n";
          var exifIfd0Directory = directories.OfType<ExifIfd0Directory>().FirstOrDefault();
          var software = exifIfd0Directory?.GetDescription(ExifDirectoryBase.TagSoftware);
          output += "Software:" + software + "\n";

          return output;
        }

        public string ImageOutput() {
          if (directories == null) return null;
          string output = "Image\n";
          var exifIfd0IfdDirectory = directories.OfType<ExifIfd0Directory>().FirstOrDefault();
          var ImageWidth = exifIfd0IfdDirectory?.GetDescription(ExifDirectoryBase.TagImageWidth);
          output += "Image Width:" + ImageWidth + "\n";
          var ImageHeight = exifIfd0IfdDirectory?.GetDescription(ExifDirectoryBase.TagImageHeight);
          output += "Image Height:" + ImageHeight + "\n";

          var subIfdDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
          var ColorSpace = subIfdDirectory?.GetDescription(ExifDirectoryBase.TagColorSpace);
          output += "Color Space:" + ColorSpace + "\n";

          var CompressedBitsPerPixel =
            subIfdDirectory?.GetDescription(ExifDirectoryBase.TagCompressedAverageBitsPerPixel);
          output += "Compressed Bits Per Pixel:" + CompressedBitsPerPixel + "\n";

          return output;
        }

        public string CameraOutput() {
          if (directories == null) return null;
          string output = "Camera\n";
          var exifIfd0IfdDirectory = directories.OfType<ExifIfd0Directory>().FirstOrDefault();
          var Make = exifIfd0IfdDirectory?.GetDescription(ExifDirectoryBase.TagMake);
          output += "Make:" + Make + "\n";
          var Model = exifIfd0IfdDirectory?.GetDescription(ExifDirectoryBase.TagModel);
          output += "Model:" + Model + "\n";

          var subIfdDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
          var ApertureValue = subIfdDirectory?.GetDescription(ExifDirectoryBase.TagAperture);
          output += "Aperture Value:" + ApertureValue + "\n";
          var ExposureTime = subIfdDirectory?.GetDescription(ExifDirectoryBase.TagExposureTime);
          output += "Exposure Time:" + ExposureTime + "\n";
          var ISOSpeedRatings = subIfdDirectory?.GetDescription(ExifDirectoryBase.TagIsoEquivalent);
          output += "ISO Speed Ratings:" + ISOSpeedRatings + "\n";
          var ExposureBiasValue = subIfdDirectory?.GetDescription(ExifDirectoryBase.TagExposureBias);
          output += "Exposure Bias Value:" + ExposureBiasValue + "\n";
          var FocalLength = subIfdDirectory?.GetDescription(ExifDirectoryBase.TagFocalLength);
          output += "Focal Length:" + FocalLength + "\n";
          var MaxApertureValue = subIfdDirectory?.GetDescription(ExifDirectoryBase.TagMaxAperture);
          output += "Max Aperture Value:" + MaxApertureValue + "\n";
          var MeteringMode = subIfdDirectory?.GetDescription(ExifDirectoryBase.TagMeteringMode);
          output += "Metering Mode:" + MeteringMode + "\n";
          var Flash = subIfdDirectory?.GetDescription(ExifDirectoryBase.TagFlash);
          output += "Flash:" + Flash + "\n";
          var FocalLength35 = subIfdDirectory?.GetDescription(ExifDirectoryBase.Tag35MMFilmEquivFocalLength);
          output += "Focal Length 35:" + FocalLength35 + "\n";

          return output;
        }

        public string FileOutput() {
          if (directories == null) return null;
          string output = "File";
          var fileMetadataDirectory = directories.OfType<FileMetadataDirectory>().FirstOrDefault();
          var FileName = fileMetadataDirectory?.GetDescription(ExifDirectoryBase.TagDocumentName);
          output += "File Name:" + FileName + "\n";

          var FileType = fileMetadataDirectory?.GetDescription(ExifDirectoryBase.TagSubfileType);
          output += "File Type:" + FileType + "\n";

          var FileModifiedDate = fileMetadataDirectory?.GetDescription(ExifDirectoryBase.TagDateTime);
          output += "File Modified Date:" + FileType + "\n";

          var FileSize = fileMetadataDirectory?.GetDescription(ExifDirectoryBase.TagFileSource);
          output += "File Size:" + FileSize + "\n";

          return output;
        }

    }
}
