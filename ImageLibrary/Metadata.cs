using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.FileSystem;
using Directory = MetadataExtractor.Directory;

namespace ImageLibrary {
  public class Metadata {
    public IEnumerable<Directory> Directories { get; private set; }

    public Metadata(string imagePath) {
      if (File.Exists(imagePath)) {
        Directories = ImageMetadataReader.ReadMetadata(imagePath);
      }
    }

    public IEnumerable<Tuple<string, string>> BasicData() {
      if (Directories == null) {
        return null;
      }

      var output = new List<Tuple<string, string>>();

      var fileMetadataDirectory = Directories.OfType<FileMetadataDirectory>().FirstOrDefault();
      var fileName = fileMetadataDirectory?.GetDescription(ExifDirectoryBase.TagDocumentName);
      if (!string.IsNullOrEmpty(fileName)) {
        output.Add(new Tuple<string, string>("Basic_Info_Filename", fileName));
      }

      var fileSize = fileMetadataDirectory?.GetDescription(ExifDirectoryBase.TagFileSource);
      if (!string.IsNullOrEmpty(fileSize)) {
        output.Add(new Tuple<string, string>("Basic_Info_Filesize", fileSize));
      }

      var modifiedDate = fileMetadataDirectory?.GetDescription(ExifDirectoryBase.TagDateTime);
      if (!string.IsNullOrEmpty(modifiedDate)) {
        output.Add(new Tuple<string, string>("Basic_Info_Modified_Date", modifiedDate));
      }

      return output;
    }

    // JPG image info retrieve
    // string output = "";
    // var exifIfd0IfdDirectory = directories.OfType<ExifIfd0Directory>().FirstOrDefault();
    // var ImageWidth = exifIfd0IfdDirectory?.GetDescription(ExifDirectoryBase.TagImageWidth);
    // output += "Image Width:" + ImageWidth + "\n";
    // var ImageHeight = exifIfd0IfdDirectory?.GetDescription(ExifDirectoryBase.TagImageHeight);
    // output += "Image Height:" + ImageHeight + "\n";
    //
    // var subIfdDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
    // var ColorSpace = subIfdDirectory?.GetDescription(ExifDirectoryBase.TagColorSpace);
    // output += "Color Space:" + ColorSpace + "\n";
    //
    // var CompressedBitsPerPixel =
    //   subIfdDirectory?.GetDescription(ExifDirectoryBase.TagCompressedAverageBitsPerPixel);
    // output += "Compressed Bits Per Pixel:" + CompressedBitsPerPixel + "\n";
    //
    // return output;

    public IEnumerable<Tuple<string, string>> ExifData() {
      if (Directories == null) {
        return null;
      }

      var output = new List<Tuple<string, string>>();

      var exifIfd0IfdDirectory = Directories.OfType<ExifIfd0Directory>().FirstOrDefault();
      var maker = exifIfd0IfdDirectory?.GetDescription(ExifDirectoryBase.TagMake);
      if (!string.IsNullOrEmpty(maker)) {
        output.Add(new Tuple<string, string>("Exif_Info_Maker", maker));
      }
      var model = exifIfd0IfdDirectory?.GetDescription(ExifDirectoryBase.TagModel);
      if (!string.IsNullOrEmpty(model)) {
        output.Add(new Tuple<string, string>("Exif_Info_Model", model));
      }

      var subIfdDirectory = Directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
      var apertureValue = subIfdDirectory?.GetDescription(ExifDirectoryBase.TagAperture);
      if (!string.IsNullOrEmpty(apertureValue)) {
        output.Add(new Tuple<string, string>("Exif_Info_Apeture_Value", apertureValue));
      }
      var exposureTime = subIfdDirectory?.GetDescription(ExifDirectoryBase.TagExposureTime);
      if (!string.IsNullOrEmpty(exposureTime)) {
        output.Add(new Tuple<string, string>("Exif_Info_Exposure_Time", exposureTime));
      }
      var isoSpeedRatings = subIfdDirectory?.GetDescription(ExifDirectoryBase.TagIsoEquivalent);
      if (!string.IsNullOrEmpty(isoSpeedRatings)) {
        output.Add(new Tuple<string, string>("Exif_Info_ISO_Speed_Ratings", isoSpeedRatings));
      }
      var exposureBiasValue = subIfdDirectory?.GetDescription(ExifDirectoryBase.TagExposureBias);
      if (!string.IsNullOrEmpty(exposureBiasValue)) {
        output.Add(new Tuple<string, string>("Exif_Info_Exposure_Bias_Value", exposureBiasValue));
      }
      var focalLength = subIfdDirectory?.GetDescription(ExifDirectoryBase.TagFocalLength);
      if (!string.IsNullOrEmpty(focalLength)) {
        output.Add(new Tuple<string, string>("Exif_Info_Focal_Length", focalLength));
      }
      var meteringMode = subIfdDirectory?.GetDescription(ExifDirectoryBase.TagMeteringMode);
      if (!string.IsNullOrEmpty(meteringMode)) {
        output.Add(new Tuple<string, string>("Exif_Info_Metering_Mode", meteringMode));
      }
      var flash = subIfdDirectory?.GetDescription(ExifDirectoryBase.TagFlash);
      if (!string.IsNullOrEmpty(flash)) {
        output.Add(new Tuple<string, string>("Exif_Info_Flash", flash));
      }

      return output;
    }
  }
}