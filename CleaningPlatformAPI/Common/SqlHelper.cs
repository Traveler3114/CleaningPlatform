using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CleaningPlatformAPI.Common;

public static class SqlHelper
{
    public static bool IsUniqueConstraintViolation(DbUpdateException exception)
    {
        for (Exception? current = exception; current is not null; current = current.InnerException)
        {
            if (current is SqlException sqlException &&
                sqlException.Errors.Cast<SqlError>().Any(error => error.Number is 2601 or 2627))
            {
                return true;
            }
        }

        return false;
    }

    public static bool IsDeadlock(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current is SqlException sqlEx && sqlEx.Number == 1205)
                return true;
        }
        return false;
    }

    public static string GetUserFriendlyMessage(SqlException sqlEx)
    {
        foreach (SqlError err in sqlEx.Errors)
        {
            switch (err.Number)
            {
                case 547:
                    return "This record cannot be deleted because it is referenced by other data. Remove the related records first, then try again.";
                case 2601:
                case 2627:
                    return "A record with this value already exists. Please use a unique value.";
                case 2628:
                    return "One of the values provided is too long for the field. Please shorten your input.";
                case 8152:
                    return "One of the values provided is too long for the field.";
                case 515:
                    return "A required field is missing. Please fill in all required fields.";
                case 4060:
                case 18456:
                    return "Database connection failed. Please contact your administrator.";
                case 1205:
                    return "A database deadlock occurred. Please try again.";
            }
        }
        return $"A database error occurred (code {sqlEx.Number}). Please try again or contact support.";
    }
}
