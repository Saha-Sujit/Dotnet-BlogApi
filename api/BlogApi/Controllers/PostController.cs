namespace Post.Controllers
{
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using CommonResponse.Models;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Post.Models;
    using Users.Data;

    [Route("api/[controller]/[action]")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly UserContext _postContext;
        private readonly IConfiguration _configuration;
        public PostController(UserContext postContext, IConfiguration configuration)
        {
            _postContext = postContext;
            _configuration = configuration;
        }

        private int GetUserIdFromClaims()
        {
            string authToken = Request.Headers.Authorization!;
            var jwtToken = authToken?.Split(" ")[1];

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.ReadJwtToken(jwtToken);
            var userId = Int32.Parse(token.Claims.FirstOrDefault()!.Value);
            return userId;
        }

        [HttpGet]
        public IActionResult GetPosts(int? id, int? userId, int? categoryId)
        {
            var response = new CommonResponse();
            IEnumerable<PostModel> posts;
            if (userId != null)
            {
                posts = _postContext.Posts.Where(item => item.UserId == userId).ToList();
            }
            else if (id != null)
            {
                posts = _postContext.Posts.Where(item => item.Id == id);
            }
            else if (categoryId != null)
            {
                posts = _postContext.Posts.Where(item => item.CategoryId == categoryId).ToList();
            }
            else
            {
                posts = _postContext.Posts.ToList();
            }

            response.statusCode = 200;
            response.message = "Your all posts is succesfully fetched";
            response.data = posts;
            return Ok(response);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddPost(PostModel post)
        {
            var response = new CommonResponse();
            var userId = GetUserIdFromClaims();
            try
            {
                var existingCategory = _postContext.Category.FirstOrDefault(item => item.Id == post.CategoryId);

                if (existingCategory == null)
                {
                    response.statusCode = 404;
                    response.message = "No category found of the id";
                    return NotFound(response);
                }

                post.UserId = userId;

                await _postContext.Posts.AddAsync(post);
                await _postContext.SaveChangesAsync();

                response.statusCode = 200;
                response.message = "Your Post is Added";

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.statusCode = 500;
                response.message = ex.Message;
                return StatusCode(500, response);
            }
        }

        [Authorize]
        [HttpPut]
        public async Task<IActionResult> UpdatePost(PostModel post)
        {
            var response = new CommonResponse();
            try
            {
                var existingPost = _postContext.Posts.FirstOrDefault(item => item.Id == post.Id);
                var userId = GetUserIdFromClaims();
                var existingCategory = _postContext.Category.FirstOrDefault(item => item.Id == post.CategoryId);

                if (existingCategory == null)
                {
                    response.statusCode = 404;
                    response.message = "No category found of the id";
                    return NotFound(response);
                }

                if (existingPost == null)
                {
                    response.statusCode = 404;
                    response.message = "No posts found of the id";
                    return NotFound(response);
                }

                if (userId != existingPost!.UserId)
                {
                    response.statusCode = 400;
                    response.message = "Sorry You are not the owner of this post";

                    return BadRequest(response);
                }

                existingPost.IsPost = post.IsPost;
                existingPost.Content = post.Content;
                existingPost.Description = post.Description;
                existingPost.IsPublished = post.IsPublished;
                existingPost.publishedDate = post.publishedDate;
                existingPost.Title = post.Title;
                existingPost.CategoryId = post.CategoryId;

                _postContext.Posts.Update(existingPost);
                await _postContext.SaveChangesAsync();

                response.statusCode = 200;
                response.message = "You Post is updated";

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.statusCode = 500;
                response.message = ex.Message;
                return StatusCode(500, response);
            }
        }

        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> DeletePost(int id)
        {
            var response = new CommonResponse();
            try
            {
                var existingPost = _postContext.Posts.FirstOrDefault(item => item.Id == id);
                if (existingPost == null)
                {
                    response.statusCode = 404;
                    response.message = "No posts found of the id";
                    return NotFound(response);
                }

                var userId = GetUserIdFromClaims();
                if (userId != existingPost!.UserId)
                {
                    response.statusCode = 400;
                    response.message = "Sorry You are not the owner of this post";

                    return BadRequest(response);
                }

                _postContext.Posts.Remove(existingPost!);
                await _postContext.SaveChangesAsync();

                response.statusCode = 200;
                response.message = "Your Post is Deleted";

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.statusCode = 500;
                response.message = ex.Message;
                return StatusCode(500, response);
            }
        }
    }
}