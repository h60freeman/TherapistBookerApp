public class ClientApiController : BaseApiController
    {

        private IClientService _service = null;

        private IAuthenticationService<int> _authService = null;

        private IUserProfileServices _profileService = null;

        public ClientApiController(IClientService service,
            ILogger<ClientApiController> logger,
            IUserProfileServices profileService,
            IAuthenticationService<int> authService) : base(logger)
        {

            _service = service;
            _authService = authService;
            _profileService = profileService;
        }

        [HttpPost]
        public ActionResult<ItemResponse<int>> Create(UserProfileAddRequest model)
        {
            ObjectResult result = null;

            try
            {
                int userId = _authService.GetCurrentUserId();

                int id = _profileService.Add(model, userId);

                ItemResponse<int> response = new ItemResponse<int>() { Item = id };

                result = Created201(response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
                
                ErrorResponse response = new ErrorResponse(ex.Message);
                
                result = StatusCode(500, response);
            }
            return result;
        }

        [HttpPost("notes")]
        public ActionResult<ItemResponse<int>> Create(ClientNoteAddRequest model)
        {
            ObjectResult result = null;

            try
            {
                int id = _service.Insert(model);    
                
                ItemResponse<int> response = new ItemResponse<int>() { Item = id };
                
                result = Created201(response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
                
                ErrorResponse response = new ErrorResponse(ex.Message);
                
                result = StatusCode(500, response);
            }
            return result;
        }

        [HttpGet("{id:int}/profile")] 
        public ActionResult<ItemResponse<Client>> GetById(int id)
        {
            int iCode = 200;
            
          BaseResponse response = null;

            try
            {
                Client client = _service.GetById(id);

                if (client == null)
                {
                    iCode = 404;
                    
                    response = new ErrorResponse("Application Resource not found.");
                }
                else
                {
                    response = new ItemResponse<Client> { Item = client };
                }
            }
            catch (Exception ex)
            {

                iCode = 500;
                
                base.Logger.LogError(ex.ToString());
                
                response = new ErrorResponse($"Generic Error: {ex.Message}");
                
            }
            return StatusCode(iCode, response);
        }

        [HttpGet("{clientId:int}/notes")]
        public ActionResult<ItemResponse<Paged<ClientNote>>> GetAllByClientId(int clientId, int pageIndex, int pageSize)
        {
            int iCode = 200;
            
            BaseResponse response = null;

            try
            {
                Paged<ClientNote>  notes = _service.GetAllByClientId(clientId, pageIndex, pageSize);

                if (notes == null)
                {
                    iCode = 404;
                    
                    response = new ErrorResponse("Application Resource not found.");
                }
                else
                {
                    response = new ItemResponse<Paged<ClientNote>> { Item = notes };
                }
            }
            catch (Exception ex)
            {
                iCode = 500;
                
                base.Logger.LogError(ex.ToString());
                
                response = new ErrorResponse($"Generic Error: {ex.Message}");
            }
            return StatusCode(iCode, response);
        }

        [HttpGet("{clientId:int}/search/notes")]
        public ActionResult<ItemResponse<Paged<ClientNote>>> SearchByClientId(int clientId, string query, int pageIndex, int pageSize)
        {
            int iCode = 200;
            
            BaseResponse response = null;

            try
            {   
                Paged<ClientNote> notes = _service.SearchByClientId(clientId, query, pageIndex , pageSize);

                if (notes == null)
                {
                    iCode = 404;
                    
                    response = new ErrorResponse("Application Resource not found.");
                }
                else
                {
                    response = new ItemResponse<Paged<ClientNote>> { Item = notes };
                }
            }
            catch (Exception ex)
            {
                iCode = 500;
                
                base.Logger.LogError(ex.ToString());
                
                response = new ErrorResponse($"Generic Error: {ex.Message}");
            }
            return StatusCode(iCode, response);
        }

        [HttpGet("sessions/{sessionId:int}/notes")]
        public ActionResult<ItemResponse<ClientNote>> GetBySessionId(int sessionId)
        {
            int iCode = 200;
            BaseResponse response = null;

            try
            {
                ClientNote note = _service.GetBySessionId(sessionId);

                if (note == null)
                {
                    iCode = 404;
                    
                    response = new ErrorResponse("Application Resource not found.");
                }
                else
                {
                    response = new ItemResponse<ClientNote> { Item = note };
                }
            }
            catch (Exception ex)
            {
                iCode = 500;
                
                base.Logger.LogError(ex.ToString());
                
                response = new ErrorResponse($"Generic Error: {ex.Message}");
            }
            return StatusCode(iCode, response);
        }     

        [HttpPut("{id:int}")]
        public ActionResult<SuccessResponse> Update(ClientUpdateRequest model)
        {
            int code = 200;
            
            BaseResponse response = null;

            try
            {
                _service.Update(model);

                response = new SuccessResponse();
            }
            catch (Exception ex)
            {
                code = 500;
                response = new ErrorResponse(ex.Message);
            }
            return StatusCode(code, response);
        }

        [HttpPut("update/{id:int}/profile")]
        public ActionResult<SuccessResponse> Update(UserProfileUpdateRequest model)
        {
            int code = 200;
            BaseResponse response = null;

            try
            {
                _service.UpdateProfile(model);

                response = new SuccessResponse();
            }
            catch (Exception ex)
            {
                code = 500;
                response = new ErrorResponse(ex.Message);
            }
            return StatusCode(code, response);
        }

        [HttpPut("{id:int}/notes")]
        public ActionResult<SuccessResponse> Update(ClientNoteUpdateRequest model)
        {
            int code = 200;
            
            BaseResponse response = null;

            try
            {
                _service.UpdateNote(model);

                response = new SuccessResponse();
            }
            catch (Exception ex)
            {
                code = 500;
                response = new ErrorResponse(ex.Message);
            }
            return StatusCode(code, response);
        }

        [HttpDelete("{noteId:int}/notes")]
        public ActionResult<SuccessResponse> Delete(int noteId)
        {
            int code = 200;
            
            BaseResponse response = null;//do not declare an instance.

            try
            {
                _service.DeleteNote(noteId);

                response = new SuccessResponse();
            }
            catch (Exception ex)
            {
                code = 500;
                
                response = new ErrorResponse(ex.Message);
            }
            return StatusCode(code, response);
        }

        [HttpGet("search")]
        public ActionResult<ItemResponse<Paged<Client>>> PagedSearch(int pageIndex, int pageSize, string query)
        {
            int code = 200;
            
            BaseResponse response = null;//do not declare an instance.

            try
            {
                Paged<Client> page = _service.PagedSearch(pageIndex, pageSize, query);

                if (page == null)
                {
                    code = 404;
                    
                    response = new ErrorResponse("App Resource not found.");
                }
                else
                {
                    response = new ItemResponse<Paged<Client>> { Item = page };
                }
            }
            catch (Exception ex)
            {
                code = 500;
                
                response = new ErrorResponse(ex.Message);
                
                base.Logger.LogError(ex.ToString());
            }
            return StatusCode(code, response);
        }

        [HttpGet("paginate")]
        public ActionResult<ItemResponse<Paged<Client>>> GetAllSummary(int pageIndex, int pageSize)
        {
            int code = 200;
            
            BaseResponse response = null;

            try
            {
                Paged<Client> page = _service.GetAllSummary(pageIndex, pageSize);

                if (page == null)
                {
                    code = 404;
                    
                    response = new ErrorResponse("App Resource not found.");
                }
                else
                {
                    response = new ItemResponse<Paged<Client>> { Item = page };
                }
            }
            catch (Exception ex)
            {
                code = 500;
                
                response = new ErrorResponse(ex.Message);
                
                base.Logger.LogError(ex.ToString());
            }
            return StatusCode(code, response);
        }
    }
