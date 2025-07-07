using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GestaoEscolarWeb.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;


namespace GestaoEscolarWeb.Data.Repositories
{
    public class SchoolClassRepository : GenericRepository<SchoolClass>, ISchoolClassRepository
    {
        private readonly DataContext _context;
        public SchoolClassRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SchoolClass>> GetAllSchoolClassesWithCourseAsync()
        {
            return await _context.SchoolClasses
                                 .Include(sc => sc.Course)
                                 .ToListAsync();
        }

        public async Task<IEnumerable<SelectListItem>> GetComboSchoolClassesAsync()
        {
            var schoolClasses = await _context.SchoolClasses
                                         .Include(sc => sc.Course)
                                         .Where(sc => sc.SchoolYear == DateTime.Now.Year || sc.SchoolYear == DateTime.Now.Year + 1) //somente ano atual ou próximo
                                         .OrderBy(sc => sc.Course.Name) // ordenar por nome do curso
                                         .ThenBy(sc => sc.SchoolYear) // depois por ano
                                         .ToListAsync();

            // converter lista para SelectListItem
            var selectListItems = schoolClasses.Select(sc => new SelectListItem
            {
                Value = sc.Id.ToString(),
                Text = sc.ToString()     // o que vou exibir na combo
            }).ToList();

            //inserir item zero fora do range como placeHolder
            selectListItems.Insert(0, new SelectListItem
            {
                Text = "Select a school class...",
                Value = "0"
            });

            return selectListItems;
        }

        public IEnumerable<SelectListItem> GetComboSchoolYears()
        {
            // Definir o ano inicial e final para a lista
            int startYear = 1900;
            int endYear = DateTime.Now.Year + 1;

            var SchoolYears = new List<SelectListItem>();

            for (int year = endYear; year >= startYear; year--) // listar do mais recente para o mais antigo, decrescente
            {
                SchoolYears.Add(new SelectListItem
                {
                    Value = year.ToString(),
                    Text = year.ToString()
                });
            }

            //primeiro item fora do range para colocar um placeholder
            SchoolYears.Insert(0, new SelectListItem
            {
                Text = "Select a year...",
                Value = "0",
                Selected = true //selecionado por default
            });

            return SchoolYears;
        }

        public IEnumerable<SelectListItem> GetComboShifts()
        {
            var selectList = new List<SelectListItem> //converter para SelectListItem
            {
                new SelectListItem{Value = "0", Text = "Select a shift..."},
                new SelectListItem{Value = "1", Text = "Morning"},
                new SelectListItem{Value = "2", Text = "Afternoon"},
                new SelectListItem{Value = "3", Text = "Night"}
            };
            return selectList;
        }

        public async Task<SchoolClass> GetSchoolClassCourseAndStudentsAsync(int id)
        {
            var schoolClass = await _context.SchoolClasses
                               .Include(sc => sc.Course)
                                    .ThenInclude(c => c.CourseSubjects)
                               .Include(sc => sc.Students)
                               .FirstOrDefaultAsync(sc => sc.Id == id);

            return schoolClass;
        }

    }
}
