using FakeItEasy;
using Kyc.Aggregation.Application.Exceptions;
using Kyc.Aggregation.Application.Models;
using Kyc.Aggregation.Application.Services;
using static Kyc.Aggregation.Application.Tests.Services.KycAggregationServiceTestHarness;

namespace Kyc.Aggregation.Application.Tests.Services;

public class KycAggregationServiceTests_AggregateData
{
    [Fact]
    public void AggregateData_MapsSsn_GivenValidInput()
    {
        // Arrange
        var sut = CreateSut();
        var input = CreateValidInput() with { Ssn = "19121212-1212" };

        // Act
        var result = sut.AggregateData(input.Ssn, input.PersonalDetails, input.ContactDetails, input.KycForm);

        // Assert
        Assert.Equal(input.Ssn, result.Ssn);
    }

    [Fact]
    public void AggregateData_MapsFirstName_GivenValidInput()
    {
        // Arrange
        var sut = CreateSut();
        var input = CreateValidInput() with
        {
            PersonalDetails = new PersonalDetailsData { FirstName = "Ada", LastName = "Lovelace" }
        };

        // Act
        var result = sut.AggregateData(input.Ssn, input.PersonalDetails, input.ContactDetails, input.KycForm);

        // Assert
        Assert.Equal("Ada", result.FirstName);
    }

    [Fact]
    public void AggregateData_MapsLastName_GivenValidInput()
    {
        // Arrange
        var sut = CreateSut();
        var input = CreateValidInput() with
        {
            PersonalDetails = new PersonalDetailsData { FirstName = "Ada", LastName = "Lovelace" }
        };

        // Act
        var result = sut.AggregateData(input.Ssn, input.PersonalDetails, input.ContactDetails, input.KycForm);

        // Assert
        Assert.Equal("Lovelace", result.LastName);
    }

    [Fact]
    public void AggregateData_MapsAddress_GivenValidInput()
    {
        // Arrange
        var sut = CreateSut();
        var input = CreateValidInput() with
        {
            ContactDetails = new ContactDetailsData { Address = "Street 1", Email = "a@b.com", PhoneNumber = "070" }
        };

        // Act
        var result = sut.AggregateData(input.Ssn, input.PersonalDetails, input.ContactDetails, input.KycForm);

        // Assert
        Assert.Equal("Street 1", result.Address);
    }

    [Fact]
    public void AggregateData_MapsPhoneNumber_WhenProvided()
    {
        // Arrange
        var sut = CreateSut();
        var input = CreateValidInput() with
        {
            ContactDetails = new ContactDetailsData { Address = "Street 1", Email = "a@b.com", PhoneNumber = "070" }
        };

        // Act
        var result = sut.AggregateData(input.Ssn, input.PersonalDetails, input.ContactDetails, input.KycForm);

        // Assert
        Assert.Equal("070", result.PhoneNumber);
    }

    [Fact]
    public void AggregateData_MapsEmail_WhenProvided()
    {
        // Arrange
        var sut = CreateSut();
        var input = CreateValidInput() with
        {
            ContactDetails = new ContactDetailsData { Address = "Street 1", Email = "a@b.com", PhoneNumber = "070" }
        };

        // Act
        var result = sut.AggregateData(input.Ssn, input.PersonalDetails, input.ContactDetails, input.KycForm);

        // Assert
        Assert.Equal("a@b.com", result.Email);
    }

    [Fact]
    public void AggregateData_MapsTaxCountry_FromLowercaseKey()
    {
        // Arrange
        var sut = CreateSut();
        var input = CreateValidInput() with { KycForm = CreateKycForm(("tax_country", "SE")) };

        // Act
        var result = sut.AggregateData(input.Ssn, input.PersonalDetails, input.ContactDetails, input.KycForm);

        // Assert
        Assert.Equal("SE", result.TaxCountry);
    }

    [Fact]
    public void AggregateData_MapsTaxCountry_FromPascalCaseKey()
    {
        // Arrange
        var sut = CreateSut();
        var input = CreateValidInput() with { KycForm = CreateKycForm(("Tax_Country", "DK")) };

        // Act
        var result = sut.AggregateData(input.Ssn, input.PersonalDetails, input.ContactDetails, input.KycForm);

        // Assert
        Assert.Equal("DK", result.TaxCountry);
    }

    [Fact]
    public void AggregateData_MapsIncome_WhenAnnualIncomeProvided()
    {
        // Arrange
        var sut = CreateSut();
        var input = CreateValidInput() with
        {
            KycForm = CreateKycForm(("tax_country", "SE"), ("annual_income", "123"))
        };

        // Act
        var result = sut.AggregateData(input.Ssn, input.PersonalDetails, input.ContactDetails, input.KycForm);

        // Assert
        Assert.Equal(123, result.Income);
    }

    [Fact]
    public void AggregateData_DoesNotMapIncome_WhenNotInteger()
    {
        // Arrange
        var sut = CreateSut();
        var input = CreateValidInput() with
        {
            KycForm = CreateKycForm(("tax_country", "SE"), ("annual_income", "not-an-int"))
        };

        // Act
        var result = sut.AggregateData(input.Ssn, input.PersonalDetails, input.ContactDetails, input.KycForm);

        // Assert
        Assert.Null(result.Income);
    }

    [Fact]
    public void AggregateData_ThrowsValidationException_WhenAddressMissing()
    {
        // Arrange
        var sut = CreateSut();
        var input = CreateValidInput() with
        {
            ContactDetails = new ContactDetailsData { Address = null, Email = "a@b.com", PhoneNumber = "070" }
        };

        // Act
        var ex = Record.Exception(() => sut.AggregateData(input.Ssn, input.PersonalDetails, input.ContactDetails, input.KycForm));

        // Assert
        Assert.Equal("Address is required but not available", (ex as ValidationException)?.Message);
    }

    [Fact]
    public void AggregateData_ThrowsValidationException_WhenTaxCountryMissing()
    {
        // Arrange
        var sut = CreateSut();
        var input = CreateValidInput() with { KycForm = CreateKycForm(("annual_income", "123")) };

        // Act
        var ex = Record.Exception(() => sut.AggregateData(input.Ssn, input.PersonalDetails, input.ContactDetails, input.KycForm));

        // Assert
        Assert.Equal("Tax country is required but not available", (ex as ValidationException)?.Message);
    }

    [Fact]
    public void AggregateData_AllowsNullEmail()
    {
        // Arrange
        var sut = CreateSut();
        var input = CreateValidInput() with
        {
            ContactDetails = new ContactDetailsData { Address = "Street 1", Email = null, PhoneNumber = "070" }
        };

        // Act
        var result = sut.AggregateData(input.Ssn, input.PersonalDetails, input.ContactDetails, input.KycForm);

        // Assert
        Assert.Null(result.Email);
    }

    [Fact]
    public void AggregateData_AllowsNullPhoneNumber()
    {
        // Arrange
        var sut = CreateSut();
        var input = CreateValidInput() with
        {
            ContactDetails = new ContactDetailsData { Address = "Street 1", Email = "a@b.com", PhoneNumber = null }
        };

        // Act
        var result = sut.AggregateData(input.Ssn, input.PersonalDetails, input.ContactDetails, input.KycForm);

        // Assert
        Assert.Null(result.PhoneNumber);
    }
}

public static class KycAggregationServiceTestHarness
{
    public sealed record AggregateDataInput(
        string Ssn,
        PersonalDetailsData PersonalDetails,
        ContactDetailsData ContactDetails,
        KycFormData KycForm);

    public static KycAggregationService CreateSut()
    {
        _ = A.Fake<object>();
        return new KycAggregationService();
    }

    public static AggregateDataInput CreateValidInput() => new(
        Ssn: "19121212-1212",
        PersonalDetails: new PersonalDetailsData { FirstName = "A", LastName = "B" },
        ContactDetails: new ContactDetailsData { Address = "Street 1", Email = "a@b.com", PhoneNumber = "070" },
        KycForm: CreateKycForm(("tax_country", "SE"), ("annual_income", "100")));

    public static KycFormData CreateKycForm(params (string Key, string Value)[] items)
    {
        var dict = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var (key, value) in items)
        {
            dict[key] = value;
        }

        return new KycFormData { Items = dict };
    }
}
