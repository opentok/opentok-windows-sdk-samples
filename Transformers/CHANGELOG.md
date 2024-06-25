# Transformers Changelog

All notable changes to this project will be documented in this file.

## 2.27.2

### Changed
Name of the prject changed from VideoTransformers to Transformers.

### Added
Noise suppresion audio transformer added to sample.

## 2.25.2

### Added

- Support pre-built transformers in the Vonage Media Processor library or create your own custom video transformer to apply to published video. NVIDIA GPUs are recommended for optimal performance.

### Known issues

- When using Vonage's Background Blur, the ML model is not present on the dll. It is required to be added manually: The steps are the following:
  - In the Solution Explorer window, right-click on the project name and select "Add" → "Existing Item.". Select _"opentok-windows-sdk-samples\VideoTransformers\Resources\selfie_segmentation.tflite"_ , then click on the "Add" button.

  - __Select the image file in the Solution Explorer, open the Properties window, and set the "Copy to Output Directory" property to either "Copy Always" or "Copy if Newer."__

### Fixed

- NA

### Enhancements

- NA

### Changed

- NA

### Deprecated

- NA
