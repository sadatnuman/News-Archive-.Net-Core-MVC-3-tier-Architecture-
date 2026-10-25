using AutoMapper;
using BLL.DTOs;
using DAL.EF.Tables;
using DAL.Repos;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Services
{
    public class CategoryService
    {
        CategoryRepo repo;
        Mapper mapper;
        public CategoryService(CategoryRepo repo)
        {
            this.repo = repo;
            mapper = MapperConfig.GetMapper();
        }
        public List<CategoryDTO> Get()
        {
            var data = repo.Get();
            var res = mapper.Map<List<CategoryDTO>>(data);
            return res;

        }
        public CategoryDTO Get(int id)
        {
            var data = repo.Get(id);
            var res = mapper.Map<CategoryDTO>(data);
            return res;

        }
        public bool Create(CategoryDTO c)
        {
            c.Slug = SlugHelper.Generate(c.Name);
            var converted = mapper.Map<Category>(c);
            return repo.Create(converted);
        }

        public bool Update(CategoryDTO c)
        {
            c.Slug = SlugHelper.Generate(c.Name);
            var converted = mapper.Map<Category>(c);
            return repo.Update(converted);
        }
        public bool Delete(int id)
        {
            if (repo.HasArticles(id)) return false;
            return repo.Delete(id);
        }

        public bool HasArticles(int id)
        {
            return repo.HasArticles(id);
        }

        public void EnsureSlugs()
        {
            var missing = repo.GetWithoutSlug();
            if (missing.Count == 0) return;
            foreach (var c in missing)
                c.Slug = SlugHelper.Generate(c.Name);
            repo.SaveChanges();
        }
    }
}
