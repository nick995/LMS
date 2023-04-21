using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LMS.Controllers
{
    public class AdministratorController : Controller
    {
        private readonly LMSContext db;

        public AdministratorController(LMSContext _db)
        {
            db = _db;
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Department(string subject)
        {
            ViewData["subject"] = subject;
            return View();
        }

        public IActionResult Course(string subject, string num)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }

        /*******Begin code to modify********/

        /// <summary>
        /// Create a department which is uniquely identified by it's subject code
        /// </summary>
        /// <param name="subject">the subject code</param>
        /// <param name="name">the full name of the department</param>
        /// <returns>A JSON object containing {success = true/false}.
        /// false if the department already exists, true otherwise.</returns>
        public IActionResult CreateDepartment(string subject, string name)
        {

            Department department = new Department();

            department.Name = name;
            department.Subject = subject;
            db.Departments.Add(department);

            try
            {
                db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception)
            {
                return Json(new { success = false });
            }
            //working
        }


        /// <summary>
        /// Returns a JSON array of all the courses in the given department.
        /// Each object in the array should have the following fields:
        /// "number" - The course number (as in 5530)
        /// "name" - The course name (as in "Database Systems")
        /// </summary>
        /// <param name="subjCode">The department subject abbreviation (as in "CS")</param>
        /// <returns>The JSON result</returns>
        public IActionResult GetCourses(string subject)
        {
            var courses =
                from course in db.Courses
                where course.Subject == subject
                select new
                {
                    number = course.Num,
                    name = course.Name

                };
            return Json(courses.ToArray());
            //working
        }

        /// <summary>
        /// Returns a JSON array of all the professors working in a given department.
        /// Each object in the array should have the following fields:
        /// "lname" - The professor's last name
        /// "fname" - The professor's first name
        /// "uid" - The professor's uid
        /// </summary>the
        /// <param name="subject">The department subject abbreviation</param>
        /// <returns>The JSON result</returns>
        public IActionResult GetProfessors(string subject)
        {
            var professors =
                from Prof in db.Professors
                where Prof.WorksIn == subject
                select new
                {
                    lname = Prof.LName,
                    fname = Prof.FName,
                    uid = Prof.UId
                };

            return Json(professors.ToArray());
            //working
        }



        /// <summary>
        /// Creates a course.
        /// A course is uniquely identified by its number + the subject to which it belongs
        /// </summary>
        /// <param name="subject">The subject abbreviation for  department in which the course will be added</param>
        /// <param name="number">The course number</param>
        /// <param name="name">The course name</param>
        /// <returns>A JSON object containing {success = true/false}.
        /// false if the course already exists, true otherwise.</returns>
        public IActionResult CreateCourse(string subject, int number, string name)
        {

            Course course = new Course();

            course.Subject = subject;
            course.Name = name;
            course.Num = (uint)number;

            db.Courses.Add(course);

            try
            {
                db.SaveChanges();
                return Json(new { success = true });

            } catch (Exception e)
            {
                return Json(new { success = false });
            }
            //working

        }



        /// <summary>
        /// Creates a class offering of a given course.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="number">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="start">The start time</param>
        /// <param name="end">The end time</param>
        /// <param name="location">The location</param>
        /// <param name="instructor">The uid of the professor</param>
        /// <returns>A JSON object containing {success = true/false}. 
        /// false if another class occupies the same location during any time 
        /// within the start-end range in the same semester, or if there is already
        /// a Class offering of the same Course in the same Semester,
        /// true otherwise.</returns>
        public IActionResult CreateClass(string subject, int number, string season, int year, DateTime start, DateTime end, string location, string instructor)
        {
            var classQuery = from classes in db.Classes
                             join tempCourse in db.Courses
                             on classes.CourseNum equals tempCourse.CourseNum
                             where classes.Semester.Equals(season) &&
                             classes.Year == year &&
                             classes.CourseNum == (uint)number
                             select new
                             {
                                 courseNum = tempCourse.CourseNum,
                                 year = classes.Year
                             } ;


            if(classQuery.Count() > 0)
            {
                return Json(new { success = false });
            }

            var conflictingClassQuery = from currClass in db.Classes
                                        where currClass.Semester.Equals(season) &&
                                        currClass.Year == year &&
                                        currClass.Loc.Equals(location)
                                        select new
                                        {
                                            startTime = currClass.StartTime,
                                            endTime = currClass.EndTime
                                        };


            foreach (var classTime in conflictingClassQuery)
            {
                if((classTime.startTime.ToTimeSpan() <= start.TimeOfDay && classTime.endTime.ToTimeSpan() >= start.TimeOfDay) ||
                    (classTime.startTime.ToTimeSpan() <= end.TimeOfDay && classTime.endTime.ToTimeSpan() >= end.TimeOfDay))
                {
                    return Json(new { success = false });
                }
            }


            var subjectQuery = from courseInstance in db.Courses
                               where courseInstance.Subject.Equals(subject)
                               && courseInstance.Num == (uint) number
                               select courseInstance.CourseNum;

            int classCourseNum = 0;
            foreach (int i  in subjectQuery)
            {
                classCourseNum = i;
            }

            Class toAdd = new Class();
            toAdd.Semester = season;
            toAdd.Year = (uint)year;
            toAdd.ProfessorId = instructor;
            toAdd.StartTime = TimeOnly.FromDateTime(start);
            toAdd.EndTime = TimeOnly.FromDateTime(end);
            toAdd.CourseNum = (uint)classCourseNum;
            toAdd.Loc = location;

            try
            {
                db.Classes.Add(toAdd);
                db.SaveChanges();
                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false });
            }         
                        
            //working
        }


        /*******End code to modify********/

    }
}

