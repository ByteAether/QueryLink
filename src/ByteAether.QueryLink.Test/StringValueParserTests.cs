namespace ByteAether.QueryLink.Test;

public class StringValueParserTests
{
	[Fact]
	public void Parse_Int_ShouldReturnInt()
	{
		var result = StringValueParser.Parse("123");
		Assert.IsType<int>(result);
		Assert.Equal(123, result);
	}

	[Fact]
	public void Parse_Double_ShouldReturnDouble()
	{
		var result = StringValueParser.Parse("123.45");
		Assert.IsType<double>(result);
		Assert.Equal(123.45, result);
	}

	[Fact]
	public void Parse_Bool_ShouldReturnBool()
	{
		var result = StringValueParser.Parse("true");
		Assert.IsType<bool>(result);
		Assert.Equal(true, result);
	}

	[Fact]
	public void Parse_Guid_ShouldReturnGuid()
	{
		var guidString = Guid.Empty.ToString();
		var result = StringValueParser.Parse(guidString);
		Assert.IsType<Guid>(result);
		Assert.Equal(guidString, result.ToString());
	}

	[Fact]
	public void Parse_TimeSpan_ShouldReturnTimeSpan()
	{
		var result = StringValueParser.Parse("1.2:3:4");
		Assert.IsType<TimeSpan>(result);
		Assert.Equal(new TimeSpan(1, 2, 3, 4), result);
	}

	[Fact]
	public void Parse_DateTime_ShouldReturnDateTime()
	{
		var result = StringValueParser.Parse("2023-10-01T12:34:56");
		Assert.IsType<DateTime>(result);
		Assert.Equal(new DateTime(2023, 10, 1, 12, 34, 56), result);
	}

	[Fact]
	public void Parse_Array_ShouldReturnArray()
	{
		var result = StringValueParser.Parse("[1, 2, 3]");
		Assert.IsType<int[]>(result);
		Assert.Equal(new int[] { 1, 2, 3 }, result);
	}

	[Fact]
	public void Parse_StringArrayWithCommas_ShouldReturnStringArray()
	{
		var result = StringValueParser.Parse("[test1, test2, test\\,with\\,comma]");
		Assert.IsType<string[]>(result);
		Assert.Equal(new string[] { "test1", "test2", "test,with,comma" }, result);
	}

	[Fact]
	public void Parse_EmptyArray_ShouldReturnArray()
	{
		var result = StringValueParser.Parse("[]");
		Assert.IsType<object[]>(result);
		Assert.Equal(new object[] { }, result);
	}

	[Fact]
	public void Parse_String_ShouldReturnString()
	{
		var result = StringValueParser.Parse("test");
		Assert.IsType<string>(result);
		Assert.Equal("test", result);
	}

	[Fact]
	public void Parse_EmptyString_ShouldReturnString()
	{
		var result = StringValueParser.Parse("");
		Assert.IsType<string>(result);
		Assert.Equal("", result);
	}

	[Fact]
	public void Parse_NullString_ShouldReturnNull()
	{
		var result = StringValueParser.Parse(null!);
		Assert.Null(result);
	}
}