public class ClientService : IClientService 
    {
        IDataProvider _data = null;
        ILocationMapper _locationMapper = null;
        IUserProfileMapper _profileMapper = null;
        IUserProfileServices _profileService = null;
        public ClientService(IDataProvider data , IUserProfileMapper profileMapper, ILocationMapper locationMapper, IUserProfileServices profileService)
        {
            _data = data;
            _locationMapper = locationMapper;
            _profileMapper = profileMapper;
            _profileService = profileService;
        }

        public int Add(ClientAddRequest model, int userId)
        {
            int id = 0;

            string procName = "[dbo].[Clients_Insert]";

            _data.ExecuteNonQuery(procName
                , inputParamMapper: delegate
                (SqlParameterCollection col)
                {
                    AddCommonParams(model, col);
                    col.AddWithValue("@UserProfileId", model.UserProfileId);
                    col.AddWithValue("@CreatedBy", userId);

                    SqlParameter idOut = new SqlParameter("@Id", SqlDbType.Int);
                    idOut.Direction = ParameterDirection.Output;

                    col.Add(idOut);
                },
                returnParameters: delegate
                (SqlParameterCollection returnCollection)
                {
                    object oId = returnCollection["@Id"].Value;

                    int.TryParse(oId.ToString(), out id);
                });
            return id;
        }

        public int Insert(ClientNoteAddRequest model)
        {
            int id = 0;

            string procName = "[dbo].[ClientNotes_Insert]";

            _data.ExecuteNonQuery(procName
                , inputParamMapper: delegate
                (SqlParameterCollection col)
                {                   
                    col.AddWithValue("@ClientId", model.ClientId);
                    col.AddWithValue("@NoteTypeId", model.NoteTypeId);
                    col.AddWithValue("@SessionId", model.SessionId);
                    col.AddWithValue("@Content", model.Content);

                    SqlParameter idOut = new SqlParameter("@Id", SqlDbType.Int);
                    idOut.Direction = ParameterDirection.Output;

                    col.Add(idOut);
                },
                returnParameters: delegate
                (SqlParameterCollection returnCollection)
                {
                    object oId = returnCollection["@Id"].Value;

                    int.TryParse(oId.ToString(), out id);
                });
                
            return id;
        }

        public void UpdateNote(ClientNoteUpdateRequest model)
        {
            string procName = "dbo.ClientNotes_Update";

            _data.ExecuteNonQuery(procName,
                inputParamMapper: delegate (SqlParameterCollection col)
                {
                    col.AddWithValue("@Id", model.Id);
                    col.AddWithValue("@Content", model.Content);
                },
                returnParameters: null
                );

        }

        public void UpdateProfile(UserProfileUpdateRequest model)
        {
            string procName = "[dbo].[UserProfiles_Update]";

            _data.ExecuteNonQuery(procName,
                inputParamMapper: delegate (SqlParameterCollection col)
                {
                    col.AddWithValue("@FirstName", model.FirstName);
                    col.AddWithValue("@LastName", model.LastName);
                    col.AddWithValue("@Mi", model.Mi);
                    col.AddWithValue("@AvatarUrl", model.AvatarUrl);
                    col.AddWithValue("@LocationId", model.LocationId);
                    col.AddWithValue("@Phone", model.Phone);
                    col.AddWithValue("@Id", model.Id);
                },
                returnParameters: null
                );

        }

        public void Update(ClientUpdateRequest model)
        {

            string procName = "[dbo].[Clients_Update]";

            _data.ExecuteNonQuery(procName,
                inputParamMapper: delegate (SqlParameterCollection col)
                {
                    AddCommonParams(model, col);
                    col.AddWithValue("@Id", model.Id);
                    col.AddWithValue("@UserProfileId", model.UserProfileId);
                },
                returnParameters: null
                ); 
        }

        public Paged<Client> GetAllSummary(int pageIndex, int pageSize)
        {
            Paged<Client> paged = null;
            List<Client> list = null;
            int totalCount = 0;

            string procName = "[dbo].[Clients_SelectAll_Summary]";

            _data.ExecuteCmd(procName,
                inputParamMapper: delegate (SqlParameterCollection col)
                {
                    col.AddWithValue("@PageIndex", pageIndex);
                    col.AddWithValue("@PageSize", pageSize);
                },
                singleRecordMapper: delegate (IDataReader reader, short set)
                {
                    Client aClient = MapDetailedClient(reader, out int index);

                    if (totalCount == 0)
                    {
                        totalCount = reader.GetSafeInt32(index);
                    }
                    if (list == null)
                    {
                        list = new List<Client>();
                    }
                    list.Add(aClient);
                });
            if (list != null)
            {
                paged = new Paged<Client>(list, pageIndex, pageSize, totalCount);
            }
            return paged;
        }

        public Client GetById(int id)
        {

            string procName = "[dbo].[Clients_SelectById_Details]";

            Client client = null;

            _data.ExecuteCmd(procName,
                delegate (SqlParameterCollection col)
                {
                    col.AddWithValue("@Id", id);
                },
               delegate (IDataReader reader, short set)
                {
                    client = MapClientProfile(reader);
                });
            return client;
        }

        public ClientNote GetBySessionId(int sessionId)
        {
            string procName = "[dbo].[ClientNotes_SelectBySessionId]";

            ClientNote note = null;

            _data.ExecuteCmd(procName,
                delegate (SqlParameterCollection col)
                {
                    col.AddWithValue("@SessionId", sessionId);
                },
               delegate (IDataReader reader, short set)
               {
                   note = MapClientNotes(reader, out int index);
               });
            return note;
        }

        public Paged<ClientNote> GetAllByClientId(int clientId, int pageIndex, int pageSize)
        {

            Paged<ClientNote> paged = null;
            List<ClientNote> list = null;
            int totalCount = 0;

            string procName = "dbo.ClientNotes_SelectByClientIdV2";

            _data.ExecuteCmd(procName,
                delegate (SqlParameterCollection col)
                {
                    col.AddWithValue("@ClientId", clientId);
                    col.AddWithValue("@PageIndex", pageIndex);
                    col.AddWithValue("@PageSize", pageSize);

                },
               delegate (IDataReader reader, short set)
               {
                   ClientNote note = MapClientNotes(reader, out int index);

                   if (totalCount == 0)
                   {
                       totalCount = reader.GetSafeInt32(index);
                   }
                   if (list == null)
                   {
                       list = new List<ClientNote>();
                   }
                   list.Add(note);
               });
            if (list != null)
            {
                paged = new Paged<ClientNote>(list, pageIndex, pageSize, totalCount);
            }
            return paged;
        }

        public Paged<ClientNote> SearchByClientId(int clientId, string query, int pageIndex, int pageSize)
        {

            Paged<ClientNote> paged = null;
            List<ClientNote> list = null;
            int totalCount = 0;


            string procName = "dbo.ClientNotes_SearchV2";

            _data.ExecuteCmd(procName,
                delegate (SqlParameterCollection col)
                {
                    col.AddWithValue("@ClientId", clientId);
                    col.AddWithValue("@Query", query);
                    col.AddWithValue("@PageIndex", pageIndex);
                    col.AddWithValue("@PageSize", pageSize);

                },
               delegate (IDataReader reader, short set)
               {
                   ClientNote note = MapClientNotes(reader, out int index);

                   if (totalCount == 0)
                   {
                       totalCount = reader.GetSafeInt32(index);
                   }
                   if (list == null)
                   {
                       list = new List<ClientNote>();
                   }
                   list.Add(note);
               });
            if (list != null)
            {
                paged = new Paged<ClientNote>(list, pageIndex, pageSize, totalCount);
            }
            return paged;
        }

        public Paged<Client> PagedSearch(int pageIndex, int pageSize, string query)
        {
            Paged<Client> paged = null;
            List<Client> list = null;
            int totalCount = 0;

            string procName = "[dbo].[Clients_Search]";

            _data.ExecuteCmd(procName,
                delegate (SqlParameterCollection col)
                {
                    col.AddWithValue("@PageIndex", pageIndex);
                    col.AddWithValue("@PageSize", pageSize);
                    col.AddWithValue("@Query", query);
                },
                singleRecordMapper: delegate (IDataReader reader, short set)
                {
                    Client aClient = MapDetailedClient(reader, out int index);

                    if (totalCount == 0)
                    {
                        totalCount = reader.GetSafeInt32(index);
                    }
                    if (list == null)
                    {
                        list = new List<Client>();
                    }
                    list.Add(aClient);
                });
            if (list != null)
            {
                paged = new Paged<Client>(list, pageIndex, pageSize, totalCount);
            }
            return paged;
        }

        public void DeleteNote(int id)
        {
            string procName = "dbo.ClientNotes_Delete";

            _data.ExecuteNonQuery(procName,
                inputParamMapper: delegate (SqlParameterCollection col) {

                    col.AddWithValue("@Id", id);
                },
                returnParameters: null);
        }

        private Client MapDetailedClient(IDataReader reader, out int startingIndex)
        {
            Client aClient = new Client();

            startingIndex = 0;

            aClient.Id = reader.GetSafeInt32(startingIndex++);
            aClient.IsActive = reader.GetSafeBool(startingIndex++);
            aClient.Notes = reader.GetSafeString(startingIndex++);

            aClient.UserProfile = _profileMapper.MapUserProfile(reader, ref startingIndex);

            return aClient;
        }

        private Client MapClientProfile(IDataReader reader)
        {

            Client client = new Client();

            int index = 0;


            client.Id = reader.GetSafeInt32(index++);
            client.IsActive = reader.GetSafeBool(index++);
            client.Notes = reader.GetSafeString(index++);

            client.UserProfile = _profileMapper.MapUserProfile(reader, ref index);

            client.IntakeProfiles = reader.DeserializeObject <List<IntakeProfile>>(index++);


            return client;
        }

        private ClientNote MapClientNotes(IDataReader reader, out int index)
        {
            ClientNote note = new ClientNote();

            index = 0;

            note.Id = reader.GetSafeInt32(index++);
            note.ClientId = reader.GetSafeInt32(index++);
            note.NoteTypeId = reader.GetSafeInt32(index++);
            note.SessionId = reader.GetSafeInt32(index++);
            note.Content = reader.GetSafeString(index++);
            note.DateCreated = reader.GetSafeDateTime(index++);

            return note;
        }

        private static void AddCommonParams(ClientAddRequest model, SqlParameterCollection col)
        {
            col.AddWithValue("@IntakeProfile", model.IntakeProfile);
            col.AddWithValue("@IsActive", model.IsActive);
            col.AddWithValue("@Notes", model.Notes);
        }
    }
