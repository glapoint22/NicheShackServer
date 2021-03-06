﻿using System;
using System.Collections.Generic;

namespace Services.Classes
{


    public enum QueryType
    {
        None,
        Category,
        Niche,
        ProductSubgroup,
        FeaturedProducts,
        ProductPrice,
        ProductRating,
        ProductKeywords,
        ProductCreationDate,
        SubQuery,
        Auto
    }



    public enum ComparisonOperatorType
    {
        Equal = 1,
        NotEqual,
        GreaterThan,
        GreaterThanOrEqual,
        LessThan,
        LessThanOrEqual,
    }


    public enum LogicalOperatorType
    {
        And = 1,
        Or
    }






    public class Query
    {
        public QueryType QueryType { get; set; }
        public ComparisonOperatorType ComparisonOperator { get; set; }
        public LogicalOperatorType LogicalOperator { get; set; }
        public int IntValue { get; set; }
        public List<int> IntValues { get; set; }
        public string StringValue { get; set; }
        public List<string> StringValues { get; set; }
        public double DoubleValue { get; set; }
        public DateTime DateValue { get; set; }
        public List<Query> SubQueries { get; set; }
    }
}
