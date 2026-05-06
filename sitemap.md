# FishingBuddy Semantic Route Model

This sitemap includes accessible MVC URLs mapped to controller/action and their backing Razor view files.

## HomeController
- `/`
  - Controller/Action: `HomeController.Index`
  - View: `Views/Home/Index.cshtml`
- `/Home/Index`
  - Controller/Action: `HomeController.Index`
  - View: `Views/Home/Index.cshtml`
- `/Home/Privacy`
  - Controller/Action: `HomeController.Privacy`
  - View: `Views/Home/Privacy.cshtml`
- `/Home/Error`
  - Controller/Action: `HomeController.Error`
  - View: `Views/Shared/Error.cshtml`

## FishController
Default route:
- `/Fish`
  - Controller/Action: `FishController.Index`
  - View: `Views/Fish/Index.cshtml`
- `/Fish/Details/{id}`
  - Controller/Action: `FishController.Details`
  - View: `Views/Fish/Details.cshtml`

Custom route (`fish-catalog`):
- `/catalog/fish`
  - Controller/Action: `FishController.Index`
  - View: `Views/Fish/Index.cshtml`
- `/catalog/fish/Index`
  - Controller/Action: `FishController.Index`
  - View: `Views/Fish/Index.cshtml`
- `/catalog/fish/Details/{id}`
  - Controller/Action: `FishController.Details`
  - View: `Views/Fish/Details.cshtml`

## BaitController
Default route:
- `/Bait`
  - Controller/Action: `BaitController.Index`
  - View: `Views/Bait/Index.cshtml`
- `/Bait/Details/{id}`
  - Controller/Action: `BaitController.Details`
  - View: `Views/Bait/Details.cshtml`

Custom route (`bait-catalog`):
- `/catalog/baits`
  - Controller/Action: `BaitController.Index`
  - View: `Views/Bait/Index.cshtml`
- `/catalog/baits/Index`
  - Controller/Action: `BaitController.Index`
  - View: `Views/Bait/Index.cshtml`
- `/catalog/baits/Details/{id}`
  - Controller/Action: `BaitController.Details`
  - View: `Views/Bait/Details.cshtml`

## UserController
Default route:
- `/User`
  - Controller/Action: `UserController.Index`
  - View: `Views/User/Index.cshtml`
- `/User/Details/{id}`
  - Controller/Action: `UserController.Details`
  - View: `Views/User/Details.cshtml`

Custom route (`angler-hub`):
- `/community/anglers`
  - Controller/Action: `UserController.Index`
  - View: `Views/User/Index.cshtml`
- `/community/anglers/Index`
  - Controller/Action: `UserController.Index`
  - View: `Views/User/Index.cshtml`
- `/community/anglers/Details/{id}`
  - Controller/Action: `UserController.Details`
  - View: `Views/User/Details.cshtml`

## FishingSpotController
Default route:
- `/FishingSpot`
  - Controller/Action: `FishingSpotController.Index`
  - View: `Views/FishingSpot/Index.cshtml`
- `/FishingSpot/Details/{id}`
  - Controller/Action: `FishingSpotController.Details`
  - View: `Views/FishingSpot/Details.cshtml`

Custom route (`spot-guide`):
- `/destinations/spots`
  - Controller/Action: `FishingSpotController.Index`
  - View: `Views/FishingSpot/Index.cshtml`
- `/destinations/spots/Index`
  - Controller/Action: `FishingSpotController.Index`
  - View: `Views/FishingSpot/Index.cshtml`
- `/destinations/spots/Details/{id}`
  - Controller/Action: `FishingSpotController.Details`
  - View: `Views/FishingSpot/Details.cshtml`

## TechniqueController
- `/Technique`
  - Controller/Action: `TechniqueController.Index`
  - View: `Views/Technique/Index.cshtml`
- `/Technique/Details/{id}`
  - Controller/Action: `TechniqueController.Details`
  - View: `Views/Technique/Details.cshtml`

## CatchRecordController
- `/CatchRecord`
  - Controller/Action: `CatchRecordController.Index`
  - View: `Views/CatchRecord/Index.cshtml`
- `/CatchRecord/Details/{id}`
  - Controller/Action: `CatchRecordController.Details`
  - View: `Views/CatchRecord/Details.cshtml`
