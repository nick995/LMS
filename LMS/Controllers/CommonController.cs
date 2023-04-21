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
    public class CommonController : Controller
    {
        private readonly LMSContext db;

        public CommonController(LMSContext _db)
        {
            db = _db;
        }

        /*******Begin code to modify********/



        /// <summary>
        /// Retreive a JSON array of all departments from the database.
        /// Each object in the array should have a field called "name" and "subject",
        /// where "name" is the department name and "subject" is the subject abbreviation.
        /// </summary>
        /// <returns>The JSON array</returns>
        public IActionResult GetDepartments()
        {
            var deps = from dep in db.Departments
                       select new
                       {
                           name = dep.Name,
                           subject = dep.Subject
                       };
            return Json(deps.ToArray());
            //working
        }

        /// <summary>
        /// Returns a JSON array representing the course catalog.
        /// Each object in the array should have the following fields:
        /// "subject": The subject abbreviation, (e.g. "CS")
        /// "dname": The department name, as in "Computer Science"
        /// "courses": An array of JSON objects representing the courses in the department.
        ///            Each field in this inner-array should have the following fields:
        ///            "number": The course number (e.g. 5530)
        ///            "cname": The course name (e.g. "Database Systems")
        /// </summary>
        /// <returns>The JSON array</returns>
        public IActionResult GetCatalog()
        {

            var catalogQuery = from dep in db.Departments
                               select new
                               {
                                   subject = dep.Subject,
                                   dname = dep.Name,
                                   courses = (from temp_course in db.Courses
                                              where temp_course.Subject.Equals(dep.Subject)
                                              select new
                                              {
                                                  number = temp_course.Num,
                                                  cname = temp_course.Name
                                              }).ToArray()
                                };

            return Json(catalogQuery.ToArray());
            //working
        }

        /// <summary>
        /// Returns a JSON array of all class offerings of a specific course.
        /// Each object in the array should have the following fields:
        /// "season": the season part of the semester, such as "Fall"
        /// "year": the year part of the semester
        /// "location": the location of the class
        /// "start": the start time in format "hh:mm:ss"
        /// "end": the end time in format "hh:mm:ss"
        /// "fname": the first name of the professor
        /// "lname": the last name of the professor
        /// </summary>
        /// <param name="subject">The subject abbreviation, as in "CS"</param>
        /// <param name="number">The course number, as in 5530</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetClassOfferings(string subject, int number)
        {

            var courseQuery = from Course in db.Courses
                              where Course.Subject == subject
                              && Course.Num == number
                              select new
                              {
                                  courseNumber = Course.CourseNum
                              };
            uint targetCourseNum = courseQuery.First().courseNumber;

            var classQuery = from temp_class in db.Classes
                             join temp_professor in db.Professors
                             on temp_class.ProfessorId equals temp_professor.UId
                             into class_professor
                             from cp_pair in class_professor.DefaultIfEmpty()
                             where targetCourseNum == temp_class.CourseNum
                             select new
                             {
                                 season = temp_class.Semester,
                                 year = temp_class.Year,
                                 location = temp_class.Loc,
                                 start = temp_class.StartTime,
                                 end = temp_class.EndTime,
                                 fname = cp_pair.FName,
                                 lname = cp_pair.LName
                             };
            return Json(classQuery.ToArray());
            //working
        }

        /// <summary>
        /// This method does NOT return JSON. It returns plain text (containing html).
        /// Use "return Content(...)" to return plain text.
        /// Returns the contents of an assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment in the category</param>
        /// <returns>The assignment contents</returns>
        public IActionResult GetAssignmentContents(string subject, int num, string season, int year, string category, string asgname)
        {
            var courseNumQuery = from temp_course in db.Courses
                                 join temp_class in db.Classes
                                 on new { A = temp_course.CourseNum, B = season, C = (uint)year } equals new { A = temp_class.CourseNum, B = temp_class.Semester, C = temp_class.Year }
                                 into course_class
                                 from cc_pair in course_class.DefaultIfEmpty()
                                 join temp_assignCat in db.AssignmentCategories
                                 on new { X = cc_pair.ClassNum, Y = category } equals new { X = temp_assignCat.ClassNum, Y = temp_assignCat.Name }
                                 into assigncat_class
                                 from cca_pair in assigncat_class.DefaultIfEmpty()
                                 join temp_assign in db.Assignments
                                 on new { Z = cca_pair.CategoryNum, W = asgname } equals new { Z = temp_assign.CategoryNum, W = temp_assign.Name }
                                 into finalTable
                                 from contentTable in finalTable.DefaultIfEmpty()
                                 where temp_course.Subject.Equals(subject) && temp_course.Num == num
                                 select contentTable.Contents;
            return Content(courseNumQuery.First());
            //working
        }


        /// <summary>
        /// This method does NOT return JSON. It returns plain text (containing html).
        /// Use "return Content(...)" to return plain text.
        /// Returns the contents of an assignment submission.
        /// Returns the empty string ("") if there is no submission.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment in the category</param>
        /// <param name="uid">The uid of the student who submitted it</param>
        /// <returns>The submission text</returns>
        public IActionResult GetSubmissionText(string subject, int num, string season, int year, string category, string asgname, string uid)
        {
            var submissionQuery = from temp_course in db.Courses
                                  join temp_class in db.Classes
                                  on new { A = temp_course.CourseNum, B = season, C = (uint)year } equals new { A = temp_class.CourseNum, B = temp_class.Semester, C = temp_class.Year }
                                  into course_class
                                  from cc_pair in course_class.DefaultIfEmpty()
                                  join temp_assignCat in db.AssignmentCategories
                                  on new { X = cc_pair.ClassNum, Y = category } equals new { X = temp_assignCat.ClassNum, Y = temp_assignCat.Name }
                                  into assigncat_class
                                  from cca_pair in assigncat_class.DefaultIfEmpty()
                                  join temp_assign in db.Assignments
                                  on new { Z = cca_pair.CategoryNum, W = asgname } equals new { Z = temp_assign.CategoryNum, W = temp_assign.Name }
                                  into ccaa_pair
                                  from contentTable in ccaa_pair.DefaultIfEmpty()
                                  join temp_submission in db.Submissions
                                  on new { U = contentTable.AssignmentNum, S = uid } equals new { U = temp_submission.AssignmentNum, S = temp_submission.UId }
                                  into submission_class
                                  from final_table in submission_class.DefaultIfEmpty()
                                  where subject.Equals(temp_course.Subject) && num == temp_course.Num
                                  select final_table.Contents;

            if(submissionQuery.Count() > 0)
            {
                return Content(submissionQuery.First());
            }
            else
            {
                return Content("");
            }
            //working
        }


        /// <summary>
        /// Gets information about a user as a single JSON object.
        /// The object should have the following fields:
        /// "fname": the user's first name
        /// "lname": the user's last name
        /// "uid": the user's uid
        /// "department": (professors and students only) the name (such as "Computer Science") of the department for the user. 
        ///               If the user is a Professor, this is the department they work in.
        ///               If the user is a Student, this is the department they major in.    
        ///               If the user is an Administrator, this field is not present in the returned JSON
        /// </summary>
        /// <param name="uid">The ID of the user</param>
        /// <returns>
        /// The user JSON object
        /// or an object containing {success: false} if the user doesn't exist
        /// </returns>
        public IActionResult GetUser(string uid)
        {
            try
            {
                var studentInfo = from temp_student in db.Students
                                  where temp_student.UId.Equals(uid)
                                  select new
                                  {
                                      fname = temp_student.FName,
                                      lname = temp_student.LName,
                                      uid = temp_student.UId,
                                      department = temp_student.Major
                                  };
                if (studentInfo.Count() > 0)
                {
                    return Json(studentInfo.Single());
                }

                var professorInfo = from temp_professor in db.Professors
                                    where temp_professor.UId.Equals(uid)
                                    select new
                                    {
                                        fname = temp_professor.FName,
                                        lname = temp_professor.LName,
                                        uid = temp_professor.UId,
                                        department = temp_professor.WorksIn
                                    };
                if (professorInfo.Count() > 0)
                {
                    return Json(professorInfo.Single());
                }

                var admin = from temp_admin in db.Administrators
                            where temp_admin.UId.Equals(uid)
                            select new
                            {
                                fname = temp_admin.FName,
                                lname = temp_admin.LName,
                                uid = temp_admin.UId,
                            };
                if (admin.Count() > 0)
                {
                    return Json(admin.Single());
                }
            }
            catch (Exception)
            {
                return Json(new { success = false });
            }

            return Json(new { success = false });
        }


        /*******End code to modify********/
    }
}

