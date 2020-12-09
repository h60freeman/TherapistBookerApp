import React from "react";
import ClientIntakeProfile from "./ClientIntakeProfiles";
import ClientSession from "./AClientSession";
import PropTypes from "prop-types";
import * as clientService from "../../services/clientService";
import * as usersService from "../../services/usersService";
import * as sessionService from "../../services/sessionService";
import * as dashboardService from "../../services/dashboardService";
import profilePic from "../../../src/assets/images/avtar/Profile.png";
import {
  Card,
  CardBody,
  CardSubtitle,
  Button,
  CardTitle,
  CardText,
  Col,
  Row,
} from "reactstrap";
import * as dateService from "../../services/dateService";
import { withRouter } from "react-router-dom";
import Pagination from "rc-pagination";

class ClientProfile extends React.Component {
  state = {
    clientId: 0,
    client: {
      userProfile: {
        firstName: "",
        lastName: "",
        middleIntial: "",
        userId: "",
        avatarUrl: "",
        location: {
          lineOne: "",
          lineTwo: "",
          city: "",
          zip: "",
        },
      },
    },
    userEmail: "",
    intakeProfiles: [],
    intakePageSize: 5,
    intakePageIndex: 1,
    intakeTotal: 0,
    profileList: [],
    allergyList: [],
    allergyNotes: "",
    latestNote: {},
    sessions: [],
    sessionList: [],
    sessionPageIndex: 1,
    sessionPageSize: 5,
    sessionTotal: 10,
    clientStatus: false,
  };

  componentDidMount = () => {
    let clientId = 0;
    let userId = 0;
    let clientStatus = false;

    if (this.props.match.params) {
      clientId = this.props.match.params.id;

      this.setState({ clientId, clientStatus }, () => this.renderAll(clientId));
    }
    if (this.props.location.pathname === "/profile") {
      userId = this.props.location.state.currentUserId;

      dashboardService
        .getClientIdByUserId(userId)
        .then(this.onGetClientIdSuccess)
        .catch(this.onGetClientIdFail);
    }
    if (this.props.location.pathname === "/dashboard") {
      clientId = this.props.clientId;

      clientStatus = true;

      this.setState({ clientId, clientStatus }, () => this.renderAll());
    }
  };

  onGetClientIdSuccess = (response) => {
    let clientId = response.item;
    let clientStatus = true;
    this.setState({ clientId, clientStatus }, () => this.renderAll());
  };

  checkRoles = (arr, val) => {
    return arr.some((arrVal) => val === arrVal);
  };

  renderAll = () => {
    this.renderClient();

    this.getNotes();

    this.getSessions();
  };

  getSessions = () => {
    let clientId = this.state.clientId;

    sessionService
      .getSessionByClientId(
        clientId,
        this.state.sessionPageIndex - 1,
        this.state.sessionPageSize
      )
      .then(this.onGetSessionSuccess)
      .catch(this.onGetSessionError);
  };

  onGetSessionSuccess = (response) => {
    let sessions = response.item.pagedItems;
    let sessionList = sessions.map(this.mapSessions);
    let sessionTotal = response.item.totalCount;

    this.setState(() => ({ sessions, sessionList, sessionTotal }));
  };

  mapSessions = (aSession) => (
    <ClientSession
      key={aSession.id}
      session={aSession}
      addNote={this.addSessionNote}
      isClient={this.state.clientStatus}
    ></ClientSession>
  );

  onGetSessionError = () => {};

  getNotes = () => {
    let clientId = this.state.clientId;

    clientService
      .getNotesById(clientId, 0, 1)
      .then(this.onGetNotesSuccess)
      .catch(this.onGetNotesError);
  };

  onGetNotesSuccess = (response) => {
    const latestNote = response.item.pagedItems[0];

    this.setState(() => ({ latestNote }));
  };

  onGetNotesError = () => {
    const latestNote = null;

    this.setState(() => ({ latestNote }));
  };

  renderClient = () => {
    let clientId = this.state.clientId;

    clientService
      .getById(clientId)
      .then(this.onGetSuccess)
      .catch(this.onGetError);
  };

  onGetSuccess = (response) => {
    const client = response.item;

    let intakeProfiles = null;
    let profileList = null;
    let intakeTotal = null;

    if (client.intakeProfiles) {
      intakeProfiles = client.intakeProfiles;

      profileList = intakeProfiles.map(this.mapProfiles);

      intakeTotal = intakeProfiles.length - 1;
    }

    this.setState(
      (prevState) => ({
        ...prevState,
        client,
        intakeProfiles,
        profileList,
        intakeTotal,
      }),
      () => this.getEmail()
    );
  };

  getAllergies = () => {
    const profileNumber = this.state.intakeTotal - 1;
    let allergyList = null;
    let allergyNotes = null;

    if (this.state.intakeProfiles) {
      if (this.state.intakeProfiles[profileNumber].allergies) {
        let allergies = this.state.intakeProfiles[profileNumber].allergies;

        allergyList = allergies.map(this.mapAllergyList);
      } else allergyList = <li>No disclosed allergies</li>;

      allergyNotes = this.state.intakeProfiles[profileNumber].allergyNotes;
    }

    this.setState(() => ({ allergyList, allergyNotes }));
  };

  mapAllergyList = (allergy) => <li key={allergy.name}>{allergy.name}</li>;

  getEmail = () => {
    const userId = this.state.client.userProfile.userId;
    usersService
      .getUserById(userId)
      .then(this.onGetUserSuccess)
      .catch(this.onGetUserFail);
  };

  onGetUserSuccess = (response) => {
    const userEmail = response.item.email;

    this.setState(
      () => ({ userEmail }),
      () => this.getAllergies()
    );
  };

  onGetUserFail = () => {};

  onGetError = () => {
    //return error message
  };

  mapProfiles = (intakeProfile) => (
    <ClientIntakeProfile
      key={intakeProfile.id}
      intakeProfile={intakeProfile}
    ></ClientIntakeProfile>
  );

  handleNotesClick = () => {
    const id = this.state.clientId;
    const clientInfo = { ...this.state.client };
    clientInfo.status = this.state.clientStatus;

    if (this.state.clientStatus) {
      this.props.history.push(`/notes`, { clientInfo, id });
    } else {
      this.props.history.push(`/clients/${id}/notes`, {
        clientInfo,
        id,
        admin: true,
      });
    }
  };

  addNoteClick = () => {
    const id = this.state.clientId;
    const clientInfo = { ...this.state.client };

    this.props.history.push(`/clients/${id}/notes/form`, clientInfo);
  };

  onProfileEditClicked = () => {
    const userProfileId = this.state.client.userProfile.id;
    const userProfile = { ...this.state.client.userProfile };
    const isEdit = true;

    this.props.history.push(`/profile/update`, {
      userProfileId,
      isEdit,
      userProfile,
    });
  };

  addSessionNote = (id) => {
    const clientId = this.state.clientId;
    const clientInfo = { ...this.state.client };
    clientInfo.sessionId = id;
    this.props.history.push(`/clients/${clientId}/notes/form`, clientInfo);
  };

  onSessionPageChange = (page) => {
    this.setState({ sessionPageIndex: page }, this.getSessions);
  };

  sessionPageSizeChange = (e) => {
    let value = e.target.value;

    this.setState({ sessionPageSize: value, sessionPageIndex: 1 }, () =>
      this.getSessions()
    );
  };

  //map intake profiles
  render() {
    return (
      <React.Fragment>
        <div className="row">
          <div className="col-sm-12">
            <div className="card hovercard text-center card card-container">
              <div className="cardheader card-header" />
              <img
                src="https://sabio-training.s3-us-west-2.amazonaws.com/bodywork/b227dd3b-fd08-4959-956e-496a515f1d7d_GalaxyPhoto.jpg"
                alt="..."
                height={470}
              ></img>
              <div className="user-image">
                <div className="avatar">
                  <img
                    alt={profilePic}
                    src={this.state.client.userProfile.avatarUrl}
                    data-intro="This is Profile image"
                    className="rounded-circle"
                    width={120}
                  />
                </div>
                <div
                  className="icon-wrapper"
                  data-intro="udpate profile here"
                  onClick={this.onProfileEditClicked}
                >
                  <i style={{ cursor: "pointer" }} className="fa fa-pencil" />
                </div>
              </div>
              <div className="info">
                <div className="row">
                  <div className="order-sm-1 order-xl-0 col-sm-6 col-lg-4">
                    <div className="row">
                      <div className="col-md-6">
                        <div className="ttl-info text-left">
                          <h6>
                            <i className="fa fa-envelope" /> Email
                          </h6>
                          <span>{this.state.userEmail}</span>
                        </div>
                      </div>
                      <div className="col-md-6">
                        <div className="ttl-info text-left ttl-sm-mb-0">
                          <h6>
                            <i className="fa fa-calendar" />
                            &nbsp;&nbsp;&nbsp;Total Appointments
                          </h6>
                          {this.state.sessionTotal}
                        </div>
                      </div>
                    </div>
                  </div>
                  <div className="order-sm-0 order-xl-1 col-sm-12 col-lg-4">
                    <div className="user-designation">
                      <div className="title">
                        {this.state.client.userProfile.firstName}{" "}
                        {this.state.client.userProfile.lastName}
                      </div>
                      <div className="desc mt-2">Client</div>
                    </div>
                  </div>
                  <div className="order-sm-2 order-xl-2 col-sm-6 col-lg-4">
                    <div className="row">
                      <div className="col-md-6">
                        <div className="ttl-info text-left ttl-xs-mt">
                          <h6>
                            <i className="fa fa-phone" />
                            &nbsp;&nbsp;&nbsp;Contact
                          </h6>
                          <span>{this.state.client.userProfile.phone}</span>
                        </div>
                      </div>
                      <div className="col-md-6">
                        <div className="ttl-info text-left ttl-sm-mb-0">
                          <h6>
                            <i className="fa fa-location-arrow" />
                            &nbsp;&nbsp;&nbsp;Location
                          </h6>
                          <span>
                            {this.state.client.userProfile.location.lineOne}{" "}
                            {this.state.client.userProfile.location.lineTwo}{" "}
                            {this.state.client.userProfile.location.city}{" "}
                            {this.state.client.userProfile.location.zipCode}
                          </span>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
                <hr />
                <div className="follow">
                  <div className="row">
                    <div className="text-md-right border-right col-4">
                      <div className="follow-num counter">
                        <p>Allergies</p>
                        {this.state.allergyList}
                        <p></p>
                      </div>
                      <div className="follow-num counter">
                        <p>AllergyNotes</p>
                        {this.state.allergyNotes}
                      </div>
                      {/* <span>
                        <button className="btn-pill btn btn-outline-warning  btn-xs">
                          Update Notes
                        </button>
                      </span> */}
                    </div>
                    <div className="text-md-left col-8">
                      <div className="follow-num counter">
                        <p>Latest Note</p>
                      </div>
                      {this.state.latestNote && (
                        <Card>
                          <CardBody>
                            <CardTitle tag="h5">
                              {this.state.latestNote.noteTypeId === 2
                                ? `Session ${this.state.latestNote.sessionId} `
                                : "General "}
                              Note
                            </CardTitle>
                            <CardSubtitle tag="h6" className="mb-2 text-muted">
                              Date:{" "}
                              {dateService.formatDateTime(
                                this.state.latestNote.dateCreated
                              )}
                            </CardSubtitle>
                            <CardText>{this.state.latestNote.content}</CardText>
                          </CardBody>
                        </Card>
                      )}
                      <Row>
                        {this.state.latestNote && (
                          <Button
                            className="col-3 float-left"
                            onClick={this.handleNotesClick}
                          >
                            View All Notes
                          </Button>
                        )}
                        <Col sm="1"></Col>
                        {!this.state.clientStatus && (
                          <Button
                            className="col-3 float-left"
                            onClick={this.addNoteClick}
                          >
                            Add Note
                          </Button>
                        )}
                      </Row>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
        <div className="card">
          <div className="card-header">
            <Row>
              <Col>
                <h4 className="card-title mb-0">Sessions</h4>
              </Col>
              <Col className="text-md-right">
                <Pagination
                  className="btn mt-3 text-nowrap"
                  onChange={this.onSessionPageChange}
                  current={this.state.sessionPageIndex}
                  total={this.state.sessionTotal}
                  pageSize={this.state.sessionPageSize}
                />
                <div className="col float-right text-nowrap itemsPerPage">
                  <div className="m-3 text-right ">
                    {" "}
                    Sessions Per Page:
                    <select
                      onChange={this.sessionPageSizeChange}
                      value={this.state.sessionPageSize}
                    >
                      <option value="5">5</option>
                      <option value="10">10</option>
                      <option value="15">15</option>
                      <option value="20">20</option>
                    </select>
                  </div>
                </div>
              </Col>
            </Row>
            <div className="card-options">
              <a
                className="card-options-collapse"
                href="#javascript"
                data-toggle="card-collapse"
              >
                <i className="fe fe-chevron-up" />
              </a>
              <a
                className="card-options-remove"
                href="#javascript"
                data-toggle="card-remove"
              >
                <i className="fe fe-x" />
              </a>
            </div>
          </div>
          <div className="table-responsive">
            <table className="table card-table table-vcenter text-nowrap">
              <thead>
                <tr>
                  <th>Start Time</th>
                  <th>Duration</th>
                  <th>End Time</th>
                  <th>Count</th>
                  <th>Session Status</th>
                  {!this.state.clientStatus && <th>Notes</th>}
                  <th />
                </tr>
              </thead>
              <tbody>{this.state.sessionList}</tbody>
            </table>
          </div>
        </div>
        <div className="card">
          <div className="card-header">
            <h4 className="card-title mb-0">Intake Profiles</h4>
            <div className="col float-left text-nowrap itemsPerPage"></div>
            <div className="card-options">
              <a
                className="card-options-collapse"
                href="#javascript"
                data-toggle="card-collapse"
              >
                <i className="fe fe-chevron-up" />
              </a>
              <a
                className="card-options-remove"
                href="#javascript"
                data-toggle="card-remove"
              >
                <i className="fe fe-x" />
              </a>
            </div>
          </div>
          <div className="table-responsive">
            <table className="table card-table table-vcenter text-nowrap">
              <thead>
                <tr>
                  <th>Date</th>
                  <th>Session Goal</th>
                  <th>Has Acknowledged</th>
                  <th>Skin Rash</th>
                  <th>Acute Pain</th>
                  <th>Cold</th>
                  <th>Contagious</th>
                  <th>Injuries</th>
                  <th>Wounds</th>
                  <th>Current Medication</th>
                  <th />
                </tr>
              </thead>
              <tbody>{this.state.profileList}</tbody>
            </table>
          </div>
        </div>
      </React.Fragment>
    );
  }
}

export default withRouter(ClientProfile);
