package dataplanesvc

import (
	"context"
	"encoding/json"
	"errors"
	"net/http"

	"github.com/gorilla/mux"
	"github.com/go-kit/kit/log"
	"github.com/go-kit/kit/transport"
	httptransport "github.com/go-kit/kit/transport/http"
)

func MakeHTTPHandler(s DataPlaneService, logger log.Logger) http.Handler {
	r := mux.NewRouter()
	e := MakeServerEndpoints(s)
	options := []httptransport.ServerOption {
		httptransport.ServerErrorHandler(transport.NewLogErrorHandler(logger)),
		httptransport.ServerErrorEncoder(encodeError),
	}

	r.Methods("POST").Path("/metadata/").Handler(httptransport.NewServer(
		e.PostMetadataEndpoint,
		decodePostMetadataRequest,
		encodeResponse,
		options...,
	))

	r.Methods("GET").Path("/metadata/{id}").Handler(httptransport.NewServer(
		e.GetMetadataByIdEndpoint,
		decodeGetMetadataRequest,
		encodeResponse,
		options...,
		))

	return r
}

func decodeGetMetadataRequest(ctx context.Context, r *http.Request) (request interface{}, err error) {
	vars := mux.Vars(r)
	id, ok := vars["id"]
	if !ok {
		return nil, errors.New("failure to parse the id")
	}
	return getMetadataByIdRequest{ID: id}, nil
}

func decodePostMetadataRequest(_ context.Context, r *http.Request) (request interface{}, err error) {
	var req postMetadataRequest
	if e := json.NewDecoder(r.Body).Decode(&req.payload); e != nil {
		return nil, e
	}
	return req, nil
}

type errorer interface {
	error() error
}

// encodeResponse is the common method for encoding all responses
func encodeResponse(ctx context.Context, w http.ResponseWriter, response interface{}) error {
	if e, ok := response.(errorer); ok && e.error() != nil {
		encodeError(ctx, e.error(), w)
		return nil
	}
	w.Header().Set("Content-Type", "application/json; charset=utf-8")
	return json.NewEncoder(w).Encode(response)
}

// encodes the error response
func encodeError(_ context.Context, err error, w http.ResponseWriter) {
	if err == nil {
		panic("encodeError with nil error")
	}
	w.Header().Set("Content-Type", "application/json; charset=utf-8")
	w.WriteHeader(codeFrom(err))
	json.NewEncoder(w).Encode(map[string]interface{}{
		"error": err.Error(),
	})
}

// extracts the http status code from the error type
func codeFrom(err error) int {
	// todo classify errors differently
	return http.StatusInternalServerError
}
