using CQRS.Mediatr.Lite;

namespace VerticalSlice.Web.Api.Data;

public class VerticalSliceCommandResult(bool success, string message) : CommandResult(success, message);
