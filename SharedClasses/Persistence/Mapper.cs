using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace SharedClasses.Persistence
{

        public  class Mapper<TEntity> where TEntity : class
        {
            private  readonly Dictionary<string, Action<TEntity, object>> _propertyMappers = new Dictionary<string, Action<TEntity, object>>();
            private  Func<TEntity> _entityFactory;

            public  Mapper<TEntity> ConstructUsing(Func<TEntity> entityFactory)
            {
                _entityFactory = entityFactory;
                return this;
            }

            public Mapper<TEntity> Map<TProperty>(Expression<Func<TEntity, TProperty>> memberExpression, string FieldName, Expression<Func<object, TProperty>> converter)
            {
                var converterInput = Expression.Parameter(typeof(object), "converterInput");
                var invokeConverter = Expression.Invoke(converter, converterInput);
                var assign = Expression.Assign(memberExpression.Body, invokeConverter);
                var mapAction = Expression.Lambda<Action<TEntity, object>>(
                    assign, memberExpression.Parameters[0], converterInput).Compile();
                _propertyMappers[FieldName] = mapAction;
               
                return this;
            }
        //Need to change type property in Request to string and add parameter formatting to the method
            public TEntity MapFrom(Dictionary<string, string> Dict)
            {
            
                var instance = _entityFactory();
                foreach (var entry in Dict)
                {
                    var found = _propertyMappers.Keys.FirstOrDefault(x => x.ToLower() == entry.Key.ToLower());
                    if (found != null)
                    {
                    
                        if (_propertyMappers.ContainsKey(entry.Key))
                        {
                            _propertyMappers[entry.Key](instance, entry.Value);

                        }
                    }
                    
                }
            //foreach (var i in instance.GetType().GetProperties().ToList())
            //{
            //    var found = formats.FirstOrDefault(x => x.Value.ToLower() == i.Name.ToLower());
            //    if (found != null)
            //    {
            //        if (i.GetValue(instance) != null && found != null && found.Format != null)
            //        {
                      
            //            var exist= instance.GetType().GetProperties().Select(x=>x.GetCustomAttributes(typeof(DescriptionAttribute), true)));

            //            var ttt = instance.GetType().GetProperties().FirstOrDefault(x => ((DescriptionAttribute)i.GetCustomAttributes(typeof(DescriptionAttribute), true)[0]).Description == i.Name);
            //          if(ttt!=null)
            //            {
            //                instance.GetType().GetProperty(i.Name).SetValue(instance, string.Format(found.Format,Convert.ChangeType(i.GetValue(instance),ttt.GetType())));

            //            }
            //            else
            //            {
            //                   instance.GetType().GetProperty(i.Name).SetValue(instance,string.Format(found.Format, i.GetValue(instance)));

            //            }




            //        }
            //    }
            //}
            return instance;
            }

        /*
         * 
         * 
         */
        
       

    }

}
