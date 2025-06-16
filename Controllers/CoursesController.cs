using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GestaoEscolarWeb.Data;
using GestaoEscolarWeb.Data.Entities;
using GestaoEscolarWeb.Data.Repositories;
using GestaoEscolarWeb.Helpers;
using GestaoEscolarWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace GestaoEscolarWeb.Controllers
{
    public class CoursesController : Controller
    {
        private readonly DataContext _context;

        private readonly ISubjectRepository _subjectRepository;

        private readonly ICourseRepository _courseRepository;

        private readonly IConverterHelper _converterHelper;

        public CoursesController(DataContext context, ISubjectRepository subjectRepository, ICourseRepository courseRepository, IConverterHelper converterHelper)
        {
            _context = context;

            _subjectRepository = subjectRepository;

            _courseRepository = courseRepository;

            _converterHelper = converterHelper;
        }

        // GET: Courses
        public async Task<IActionResult> Index()
        {
            return  View(_courseRepository.GetAll().OrderBy(c => c.Name));
        }

        // GET: Courses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound(); //TODO fazer notfound personalizado 
            }

            var course = await _courseRepository.GetByIdAsync(id.Value);
            if (course == null)
            {
                return NotFound(); //TODO fazer notfound personalizado
            }

            return View(course);
        }

        // GET: Courses/Create
        public async Task<IActionResult> Create()
        {
            var model = new CourseViewModel();

            model.SubjectsToSelect = await _subjectRepository.GetSubjectsToSelectAsync();

            return View(model); // Passar a ViewModel populada
        }

        // POST: Courses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CourseViewModel model)
        {
            if (ModelState.IsValid) //TODO está dando CourseSubjects Invalid
            {
                // Criar um curso
                var course = new Course
                {
                    Name = model.Name,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    //TODO UserAudit = ... (definir quando fizer os users)
                    CourseSubjects = new List<Subject>() // Inicializar a lista de Subjects
                };

                // Adicionar as Subjects selecionadas
                if (model.SelectedSubjectIds != null && model.SelectedSubjectIds.Any())
                {
                    foreach (var subjectId in model.SelectedSubjectIds)
                    {
                        var subject = await _subjectRepository.GetByIdAsync(subjectId);
                        if (subject != null)
                        {
                            course.CourseSubjects.Add(subject); // em memória
                        }
                    }
                }

                    await _courseRepository.CreateAsync(course); //CreateAsync add na database e salva
                    return RedirectToAction(nameof(Index));

            }


            model.SubjectsToSelect = await _subjectRepository.GetSubjectsToSelectAsync();

            return View(model);
        }

        // GET: Courses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound(); //TODO fazer notfound personalizado
            }

            var course = await _courseRepository.GetCourseWithSubjectsByIdAsync(id.Value);
            if (course == null)
            {
                return NotFound(); //TODO fazer notfound personalizado
            }

            var model = _converterHelper.ToCourseViewModel(course); //converter para model

            model.SubjectsToSelect = await _subjectRepository.GetSubjectsToSelectAsync(); //popular lista de seleção de subjects

            return View(model);
        }

        // POST: Courses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CourseViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound(); //TODO fazer notfound personalizado
            }

            if (ModelState.IsValid)
            {
                var course = await _courseRepository.GetCourseWithSubjectsByIdAsync(id); //buscar curso com subjects

                //atualizar propriedades
                course.Name = model.Name;
                course.StartDate = model.StartDate;
                course.EndDate = model.EndDate;
                // course.UserAudit = model.UserAudit; // Atualizar se UserAudit quando fizer users

                // atualizar listas de subjects
                var selectedSubjectIdsSet = new List<int>(model.SelectedSubjectIds ?? new List<int>());

                //Remover Subjects que foram desmarcados
                foreach (var subjectToRemove in course.CourseSubjects.ToList()) // .ToList() para evitar "Collection was modified"
                {
                    if (!selectedSubjectIdsSet.Contains(subjectToRemove.Id))
                    {
                        course.CourseSubjects.Remove(subjectToRemove);
                    }
                }

               //Adicionar Subjects que foram marcados
                var subjectIdsInDb = new List<int>(course.CourseSubjects.Select(s => s.Id));

                foreach (var selectedId in selectedSubjectIdsSet)
                {
                    if (!subjectIdsInDb.Contains(selectedId))
                    {
                        // Buscar o Subject existente do banco de dados 
                        var subjectToAdd = await _subjectRepository.GetByIdAsync(selectedId);
                        if (subjectToAdd != null)
                        {
                            course.CourseSubjects.Add(subjectToAdd);
                        }
                    }
                }

                try
                {
                    await _courseRepository.UpdateAsync(course);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _courseRepository.ExistAsync(id))
                    {
                        return NotFound(); //TODO fazer notfound personalizado
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            // Se houver erros de validação, re-popular SubjectsToSelect antes de retornar a view
            model.SubjectsToSelect = await _subjectRepository.GetSubjectsToSelectAsync();
            return View(model);
           
        }

        // GET: Courses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            //TODO resolver o delete em cascata
            if (id == null)
            {
                return NotFound(); //TODO fazer notfound personalizado
            }

            var course = await _courseRepository?.GetByIdAsync(id.Value);
            if (course == null)
            {
                return NotFound(); //TODO fazer notfound personalizado
            }

            return View(course);
        }

        // POST: Courses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _courseRepository.GetByIdAsync(id);

            await _courseRepository.DeleteAsync(course);

            return RedirectToAction(nameof(Index));
        }

    }
}
