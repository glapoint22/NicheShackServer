﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DataAccess.Interfaces;

namespace DataAccess.Repositories
{
    public interface IRepository<T> where T: class
    {
        // Get overloads
        Task<T> Get(int id);
        Task<T> Get(string id);
        Task<T> Get(Expression<Func<T, bool>> predicate);
        Task<TOut> Get<TOut>(Expression<Func<T, bool>> predicate, Expression<Func<T, TOut>> select);
        Task<TOut> Get<TOut>(Expression<Func<T, bool>> predicate) where TOut : class, new();



        // GetCollection overloads
        Task<IEnumerable<T>> GetCollection();
        Task<IEnumerable<TOut>> GetCollection<TOut>(Expression<Func<T, bool>> predicate, Expression<Func<T, TOut>> select);
        Task<IEnumerable<TOut>> GetCollection<TOut>(Expression<Func<T, TOut>> select);
        Task<IEnumerable<TOut>> GetCollection<TOut>() where TOut : class, new();
        Task<IEnumerable<TOut>> GetCollection<TOut>(Expression<Func<T, bool>> predicate) where TOut : class, new();




        // Count
        Task<int> GetCount(Expression<Func<T, bool>> predicate);



        // Add
        void Add(T entity);

        // Update
        void Update(T entity);


        // Remove
        void Remove(T entity);


        // Any
        Task<bool> Any(Expression<Func<T, bool>> predicate);
    }
}
