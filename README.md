# SQS Toolbox

This is a work-in-progress repository for a set of libraries, extensions and helpers to support working with AWS Simple Queue Service from .NET applications.

## Planned Features

### SQS Polling Queue Reader

Support for polling an SQS queue for messages in a background `Task`.

Status: Work in progress

### SQS Batch Deleter

Support for registering messages for deletion in batches, with an optional timer that triggers the batch if the batch size has not been met.

Status: Planned
